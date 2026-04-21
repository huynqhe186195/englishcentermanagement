using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using EnglishCenter.Web.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
            var wrappedResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var loginResp = wrappedResponse?.Data;
            if (loginResp == null)
            {
                loginResp = JsonSerializer.Deserialize<LoginResponse>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }

            if (loginResp == null)
            {
                ErrorMessage = "Invalid server response.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(loginResp.AccessToken))
            {
                ErrorMessage = "Invalid server response.";
                return Page();
            }

            // store tokens in session
            HttpContext.Session.SetString("AccessToken", loginResp.AccessToken ?? string.Empty);
            HttpContext.Session.SetString("RefreshToken", loginResp.RefreshToken ?? string.Empty);
            HttpContext.Session.SetString("UserName", loginResp.UserName ?? string.Empty);
            var roles = (loginResp.Roles ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!roles.Any())
            {
                roles = ExtractRolesFromToken(loginResp.AccessToken);
            }

            HttpContext.Session.SetString("Roles", JsonSerializer.Serialize(roles));
            HttpContext.Session.SetString("CampusId", loginResp.CampusId?.ToString() ?? Input.CampusId.ToString());

            // redirect based on role
            if (roles.Contains("SUPER_ADMIN", StringComparer.OrdinalIgnoreCase)) return RedirectToPage("/SuperAdmins/Dashboard");
            if (roles.Contains("CENTER_ADMIN", StringComparer.OrdinalIgnoreCase)) return RedirectToPage("/Admin/Index");
            if (roles.Contains("TEACHER", StringComparer.OrdinalIgnoreCase)) return RedirectToPage("/Teacher/Index");
            if (roles.Contains("STAFF", StringComparer.OrdinalIgnoreCase)) return RedirectToPage("/Admin/Index");
            if (roles.Contains("PARENT", StringComparer.OrdinalIgnoreCase)) return RedirectToPage("/Student/Index");
            if (roles.Contains("STUDENT", StringComparer.OrdinalIgnoreCase)) return RedirectToPage("/Student/Index");

            // authenticated but role does not match known route yet
            return RedirectToPage("/Student/Index");
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
            const int pageSize = 100;
            var allItems = new List<CampusOption>();
            var pageNumber = 1;
            var totalPages = 1;

            do
            {
                var response = await client.GetFromJsonAsync<ApiResponse<PagedResult<CampusOption>>>(
                    $"campuses?pageNumber={pageNumber}&pageSize={pageSize}&status=1");
                var data = response?.Data;
                if (data == null) break;

                allItems.AddRange(data.Items);
                totalPages = data.TotalPages < 1 ? 1 : data.TotalPages;
                pageNumber++;
            } while (pageNumber <= totalPages);

            Campuses = allItems
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

    private static List<string> ExtractRolesFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var roles = jwt.Claims
                .Where(c =>
                    c.Type == ClaimTypes.Role ||
                    c.Type.Equals("role", StringComparison.OrdinalIgnoreCase) ||
                    c.Type.EndsWith("/claims/role", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return roles;
        }
        catch
        {
            return new List<string>();
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

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalPages { get; set; }
}

public class CampusOption
{
    public long Id { get; set; }
    public string CampusCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
