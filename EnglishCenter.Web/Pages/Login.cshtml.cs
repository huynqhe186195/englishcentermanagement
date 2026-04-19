using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishCenter.Web.Pages;

public class LoginModel : PageModel
{
    private const string ApiClientName = "EnglishCenterApi";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LoginModel> _logger;

    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public List<SelectListItem> CampusOptions { get; private set; } = [];

    public LoginModel(IHttpClientFactory httpClientFactory, ILogger<LoginModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        await LoadCampusOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCampusOptionsAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient(ApiClientName);

            var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
            {
                UserName = Input.UserName,
                Password = Input.Password
            });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Đăng nhập thất bại. Vui lòng kiểm tra lại tài khoản/mật khẩu.");
                return Page();
            }

            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);
            if (payload?.Success != true || payload.Data is null)
            {
                ModelState.AddModelError(string.Empty, payload?.Message ?? "Đăng nhập không thành công.");
                return Page();
            }

            SetCookie("ecm_access_token", payload.Data.AccessToken, payload.Data.ExpiresAtUtc);
            SetCookie("ecm_refresh_token", payload.Data.RefreshToken, payload.Data.ExpiresAtUtc.AddDays(7));
            if (Input.CampusId.HasValue)
            {
                SetCookie("ecm_campus_id", Input.CampusId.Value.ToString(), payload.Data.ExpiresAtUtc.AddDays(7));
            }

            return RedirectToPage("/Home", new { campusId = Input.CampusId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi đăng nhập");
            ModelState.AddModelError(string.Empty, "Không thể kết nối tới API đăng nhập.");
            return Page();
        }
    }

    private async Task LoadCampusOptionsAsync()
    {
        var client = _httpClientFactory.CreateClient(ApiClientName);
        var campusesResult = await GetApiDataAsync<PagedResult<CampusItem>>(client, "/api/Campuses?pageNumber=1&pageSize=100");
        var campuses = campusesResult?.Items ?? [];

        CampusOptions = campuses
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.CampusCode} - {c.Name}",
                Selected = Input.CampusId == c.Id
            })
            .ToList();

        CampusOptions.Insert(0, new SelectListItem
        {
            Value = string.Empty,
            Text = "-- Chọn campus --",
            Selected = !Input.CampusId.HasValue
        });
    }

    private void SetCookie(string key, string value, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(key, value, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Expires = new DateTimeOffset(expiresAtUtc)
        });
    }

    private static async Task<T?> GetApiDataAsync<T>(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        return payload?.Success == true ? payload.Data : default;
    }

    public sealed class LoginInputModel
    {
        [Display(Name = "Campus")]
        [Required(ErrorMessage = "Vui lòng chọn campus.")]
        public long? CampusId { get; set; }

        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public sealed class LoginRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }

    public sealed class CampusItem
    {
        public long Id { get; set; }
        public string CampusCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public sealed class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = [];
    }
}
