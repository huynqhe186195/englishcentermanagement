using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
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

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.PostAsJsonAsync("auth/login", new { UserName = Input.UserName, Password = Input.Password });
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
}

public class LoginInput
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public class LoginResponse
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public List<string>? Roles { get; set; }
}
