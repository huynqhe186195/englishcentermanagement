using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.Account;

public class CompleteStudentProfileModel : PageModel
{
    private readonly IApiClient _apiClient;

    public CompleteStudentProfileModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public long StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    public UpdateStudentProfileRequestDto ProfileForm { get; set; } = new();

    public string? Message { get; set; }
    public bool IsSuccess { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var me = await _apiClient.GetAsync<CurrentUserDto>("auth/me");
        if (me == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var isStudent = me.Roles?.Contains("STUDENT", StringComparer.OrdinalIgnoreCase) == true;
        if (!isStudent)
        {
            return RedirectToPage("/Account/Login");
        }

        StudentId = me.StudentId ?? 0;
        FullName = me.FullName;

        if (StudentId <= 0)
        {
            Message = "Không tìm thấy hồ sơ học viên liên kết.";
            return Page();
        }

        var profile = await _apiClient.GetAsync<StudentProfileDetailDto>($"students/{StudentId}");
        if (profile != null)
        {
            ProfileForm = new UpdateStudentProfileRequestDto
            {
                FullName = profile.FullName,
                DateOfBirth = profile.DateOfBirth,
                Gender = profile.Gender,
                Phone = profile.Phone,
                Email = profile.Email,
                SchoolName = profile.SchoolName,
                EnglishLevel = profile.EnglishLevel,
                Note = profile.Note,
                Status = profile.Status
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var me = await _apiClient.GetAsync<CurrentUserDto>("auth/me");
        if (me == null)
        {
            return RedirectToPage("/Account/Login");
        }

        StudentId = me.StudentId ?? 0;
        FullName = me.FullName;

        if (StudentId <= 0)
        {
            Message = "Không tìm thấy hồ sơ học viên liên kết.";
            IsSuccess = false;
            return Page();
        }

        var ok = await _apiClient.PutAsync("students/me/profile", ProfileForm);
        if (!ok)
        {
            Message = "Cập nhật hồ sơ thất bại. Vui lòng kiểm tra lại thông tin.";
            IsSuccess = false;
            return Page();
        }

        var completed = IsProfileCompleted(ProfileForm);
        HttpContext.Session.SetString("HasCompletedStudentProfile", completed ? "true" : "false");

        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return Redirect(ReturnUrl);
        }

        Message = "Cập nhật hồ sơ thành công.";
        IsSuccess = true;
        return Page();
    }

    private static bool IsProfileCompleted(UpdateStudentProfileRequestDto profile)
    {
        return !string.IsNullOrWhiteSpace(profile.FullName)
            && profile.DateOfBirth.HasValue
            && profile.Gender.HasValue
            && !string.IsNullOrWhiteSpace(profile.Phone)
            && !string.IsNullOrWhiteSpace(profile.Email)
            && !string.IsNullOrWhiteSpace(profile.SchoolName)
            && !string.IsNullOrWhiteSpace(profile.EnglishLevel)
            && profile.Status == 1;
    }
}
