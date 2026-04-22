using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishCenter.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RegisterModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.PostAsJsonAsync("auth/register-student", new
            {
                userName = Input.UserName,
                password = Input.Password,
                email = Input.Email,
                phoneNumber = Input.PhoneNumber,
                fullName = Input.FullName
            });

            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync();
                ErrorMessage = TryGetMessage(raw) ?? "Đăng ký thất bại. Vui lòng thử lại.";
                return Page();
            }

            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công. Vui lòng đăng nhập để tiếp tục.";
            return RedirectToPage("/Account/Login");
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error connecting to API: " + ex.Message;
            return Page();
        }
    }

    private static string? TryGetMessage(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            var apiResp = JsonSerializer.Deserialize<ApiResponse<object>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!string.IsNullOrWhiteSpace(apiResp?.Message))
            {
                return apiResp.Message;
            }
        }
        catch
        {
            // ignore parsing errors
        }

        return null;
    }
}

public class RegisterInput
{
    [Required]
    [MinLength(4)]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;
}
