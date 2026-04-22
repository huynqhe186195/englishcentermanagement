using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace EnglishCenter.Web.Pages.Courses;

public class DetailsModel : PageModel
{
    private readonly IApiClient _apiClient;

    public DetailsModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public CourseDto Course { get; set; } = new();
    public bool IsStudentLoggedIn { get; set; }
    public bool HasAnyEnrollment { get; set; }
    public string StudentDisplayName { get; set; } = "Học viên";
    public string ProfilePageUrl => HasAnyEnrollment ? "/Student/Profile" : "/Account/CompleteStudentProfile";

    [BindProperty(SupportsGet = true)]
    public long Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool AutoRegister { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (Id <= 0)
        {
            return RedirectToPage("/Courses/Index");
        }

        ResolveStudentSessionFlags();
        await LoadCourseAsync();

        if (Course.Id <= 0)
        {
            TempData["ErrorMessage"] = "Không tìm thấy khóa học.";
            return RedirectToPage("/Courses/Index");
        }

        if (AutoRegister && IsStudentLoggedIn && !HasAnyEnrollment)
        {
            return await ExecuteRegisterAsync(Id);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        if (Id <= 0)
        {
            return RedirectToPage("/Courses/Index");
        }

        return await ExecuteRegisterAsync(Id);
    }

    private async Task<IActionResult> ExecuteRegisterAsync(long courseId)
    {
        var hasCompletedProfileFromSession = string.Equals(
            HttpContext.Session.GetString("HasCompletedStudentProfile"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (!hasCompletedProfileFromSession)
        {
            TempData["ErrorMessage"] = "Vui lòng hoàn thiện hồ sơ cá nhân trước khi đăng ký khóa học.";
            return RedirectToPage("/Account/CompleteStudentProfile", new { returnUrl = $"/Courses/Details?id={courseId}&autoRegister=true" });
        }

        var me = await _apiClient.GetAsync<CurrentUserDto>("auth/me");
        if (me == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var isStudent = me.Roles?.Contains("STUDENT", StringComparer.OrdinalIgnoreCase) == true;
        if (!isStudent)
        {
            TempData["ErrorMessage"] = "Chỉ học viên mới có thể đăng ký khóa học.";
            return RedirectToPage("/Courses/Details", new { id = courseId });
        }

        if (!me.StudentId.HasValue || me.StudentId.Value <= 0)
        {
            TempData["ErrorMessage"] = "Bạn chưa có hồ sơ học viên. Vui lòng cập nhật hồ sơ trước.";
            return RedirectToPage("/Account/CompleteStudentProfile", new { returnUrl = $"/Courses/Details?id={courseId}&autoRegister=true" });
        }

        var profile = await _apiClient.GetAsync<StudentProfileDetailDto>($"students/{me.StudentId.Value}");
        if (!IsProfileCompleted(profile))
        {
            TempData["ErrorMessage"] = "Vui lòng hoàn thiện hồ sơ cá nhân trước khi đăng ký khóa học.";
            HttpContext.Session.SetString("HasCompletedStudentProfile", "false");
            return RedirectToPage("/Account/CompleteStudentProfile", new { returnUrl = $"/Courses/Details?id={courseId}&autoRegister=true" });
        }

        var classesPaged = await _apiClient.GetAsync<PagedResult<ClassDto>>("classes?PageNumber=1&PageSize=500&Status=1");
        var targetClass = classesPaged?.Items?
            .Where(x => x.CourseId == courseId && x.Status == 1)
            .OrderBy(x => x.StartDate)
            .FirstOrDefault();

        if (targetClass == null)
        {
            TempData["ErrorMessage"] = "Hiện chưa có lớp mở cho khóa học này.";
            return RedirectToPage("/Courses/Details", new { id = courseId });
        }

        var created = await _apiClient.PostAsync<CreateEnrollmentRequest, object>("enrollments", new CreateEnrollmentRequest
        {
            StudentId = me.StudentId.Value,
            ClassId = targetClass.Id,
            EnrollDate = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"),
            Status = 1,
            Note = "Self-enrolled from course details"
        });

        if (created == null)
        {
            TempData["ErrorMessage"] = "Đăng ký khóa học thất bại. Vui lòng thử lại.";
            return RedirectToPage("/Courses/Details", new { id = courseId });
        }

        HttpContext.Session.SetString("HasAnyEnrollment", "true");
        TempData["SuccessMessage"] = "Đăng ký khóa học thành công.";

        return RedirectToPage("/Courses/Details", new { id = courseId });
    }

    private async Task LoadCourseAsync()
    {
        Course = await _apiClient.GetAsync<CourseDto>($"courses/{Id}") ?? new CourseDto();
    }

    private void ResolveStudentSessionFlags()
    {
        var rawRoles = HttpContext.Session.GetString("Roles");
        var roles = string.IsNullOrWhiteSpace(rawRoles)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(rawRoles) ?? new List<string>();

        IsStudentLoggedIn = roles.Contains("STUDENT", StringComparer.OrdinalIgnoreCase);
        HasAnyEnrollment = string.Equals(HttpContext.Session.GetString("HasAnyEnrollment"), "true", StringComparison.OrdinalIgnoreCase);
        StudentDisplayName = HttpContext.Session.GetString("FullName")
            ?? HttpContext.Session.GetString("UserName")
            ?? "Học viên";
    }

    private static bool IsProfileCompleted(StudentProfileDetailDto? profile)
    {
        if (profile == null) return false;

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
