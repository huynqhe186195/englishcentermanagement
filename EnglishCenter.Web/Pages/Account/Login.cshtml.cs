using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EnglishCenter.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LoginModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;
    public List<SelectListItem> Campuses { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadCampusesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCampusesAsync();

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.PostAsJsonAsync("auth/login", new { UserName = Input.UserName, Password = Input.Password, CampusId = Input.CampusId });
            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Invalid credentials or server error.";
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var loginResp = apiResponse?.Data;
            if (loginResp == null)
            {
                ErrorMessage = "Invalid server response.";
                return Page();
            }

            // store tokens in session
            HttpContext.Session.SetString("AccessToken", loginResp.AccessToken ?? string.Empty);
            HttpContext.Session.SetString("RefreshToken", loginResp.RefreshToken ?? string.Empty);
            HttpContext.Session.SetString("UserName", loginResp.UserName ?? string.Empty);
            HttpContext.Session.SetString("Roles", JsonSerializer.Serialize(loginResp.Roles ?? new List<string>()));
            HttpContext.Session.SetString("CampusId", loginResp.CampusId?.ToString() ?? Input.CampusId.ToString());

            // redirect based on role
            var roles = loginResp.Roles ?? new List<string>();
            if (roles.Contains("SUPER_ADMIN")) return RedirectToPage("/Admin/Index");
            if (roles.Contains("TEACHER")) return RedirectToPage("/Teacher/Index");
            if (roles.Contains("STUDENT")) return RedirectToPage("/Student/Index");

            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error connecting to API: " + ex.Message;
            return Page();
        }
    }

    private async Task LoadCampusesAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.GetFromJsonAsync<ApiResponse<PagedResult<CampusOption>>>("campuses?pageNumber=1&pageSize=1000&status=1");
            var items = response?.Data?.Items ?? new List<CampusOption>();
            Campuses = items
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = $"{x.CampusCode} - {x.Name}"
                })
                .ToList();
        }
        catch
        {
            Campuses = new List<SelectListItem>();
        }
    }
}

public class LoginInput
{
    [Required]
    public string UserName { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "Campus is required.")]
    public long CampusId { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
}

public class CampusOption
{
    public long Id { get; set; }
    public string CampusCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class LoginResponse
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public long? CampusId { get; set; }
    public List<string>? Roles { get; set; }
}
