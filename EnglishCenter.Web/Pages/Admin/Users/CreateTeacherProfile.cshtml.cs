using System.Text.Json;
using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.Admin.Users;

public class CreateTeacherProfileModel : PageModel
{
    private readonly IApiClient _apiClient;

    public CreateTeacherProfileModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public bool IsCenterAdmin { get; set; }

    [BindProperty(SupportsGet = true)]
    public long UserId { get; set; }

    public UserDetailDto? UserInfo { get; set; }

    [BindProperty]
    public CreateTeacherProfileInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!EnsureCenterAdmin())
        {
            return RedirectToPage("/Admin/Users/Index");
        }

        if (UserId <= 0)
        {
            TempData["ErrorMessage"] = "UserId is required.";
            return RedirectToPage("/Admin/Users/Index");
        }

        UserInfo = await _apiClient.GetAsync<UserDetailDto>($"campus-admin/users/{UserId}");
        if (UserInfo == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToPage("/Admin/Users/Index");
        }

        Input.UserId = UserInfo.Id;
        Input.FullName = UserInfo.FullName;
        Input.Email = UserInfo.Email;
        Input.Phone = UserInfo.PhoneNumber;
        Input.HireDate = DateTime.Today;
        Input.Status = 1;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!EnsureCenterAdmin())
        {
            return RedirectToPage("/Admin/Users/Index");
        }

        if (Input.UserId <= 0)
        {
            TempData["ErrorMessage"] = "Invalid user.";
            return RedirectToPage("/Admin/Users/Index");
        }

        if (string.IsNullOrWhiteSpace(Input.FullName)
            || string.IsNullOrWhiteSpace(Input.Specialization)
            || string.IsNullOrWhiteSpace(Input.Qualification))
        {
            TempData["ErrorMessage"] = "FullName, Specialization, Qualification are required.";
            return RedirectToPage("./CreateTeacherProfile", new { userId = Input.UserId });
        }

        var ok = await _apiClient.PostAsync("teachers", new
        {
            userId = Input.UserId,
            fullName = Input.FullName,
            phone = Input.Phone,
            email = Input.Email,
            specialization = Input.Specialization,
            qualification = Input.Qualification,
            hireDate = Input.HireDate,
            status = Input.Status
        });

        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Teacher profile created successfully."
            : "Failed to create teacher profile.";

        return RedirectToPage("/Admin/Users/Index");
    }

    private bool EnsureCenterAdmin()
    {
        var rawRoles = HttpContext.Session.GetString("Roles");
        var roles = string.IsNullOrWhiteSpace(rawRoles)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(rawRoles) ?? new List<string>();

        IsCenterAdmin =
            roles.Contains("CENTER_ADMIN", StringComparer.OrdinalIgnoreCase)
            || roles.Contains("MANAGER", StringComparer.OrdinalIgnoreCase)
            || roles.Contains("ADMIN", StringComparer.OrdinalIgnoreCase);

        return IsCenterAdmin;
    }

    public class CreateTeacherProfileInput
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public DateTime HireDate { get; set; } = DateTime.Today;
        public int Status { get; set; } = 1;
    }
}