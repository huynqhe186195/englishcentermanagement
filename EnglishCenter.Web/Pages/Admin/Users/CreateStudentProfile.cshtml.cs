using System.Text.Json;
using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.Admin.Users;

public class CreateStudentProfileModel : PageModel
{
    private readonly IApiClient _apiClient;

    public CreateStudentProfileModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public bool IsCenterAdmin { get; set; }

    [BindProperty(SupportsGet = true)]
    public long UserId { get; set; }

    public UserDetailDto? UserInfo { get; set; }

    [BindProperty]
    public CreateStudentProfileInput Input { get; set; } = new();

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
            || string.IsNullOrWhiteSpace(Input.SchoolName)
            || string.IsNullOrWhiteSpace(Input.EnglishLevel))
        {
            TempData["ErrorMessage"] = "FullName, SchoolName, EnglishLevel are required.";
            return RedirectToPage("./CreateStudentProfile", new { userId = Input.UserId });
        }

        var ok = await _apiClient.PostAsync("students", new
        {
            userId = Input.UserId,
            fullName = Input.FullName,
            dateOfBirth = Input.DateOfBirth,
            gender = Input.Gender,
            phone = Input.Phone,
            email = Input.Email,
            schoolName = Input.SchoolName,
            englishLevel = Input.EnglishLevel,
            note = Input.Note,
            status = Input.Status
        });

        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Student profile created successfully."
            : "Failed to create student profile.";

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

    public class CreateStudentProfileInput
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; } = new DateOnly(2012, 1, 1);
        public int Gender { get; set; } = 0;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string EnglishLevel { get; set; } = string.Empty;
        public string? Note { get; set; }
        public int Status { get; set; } = 1;
    }
}