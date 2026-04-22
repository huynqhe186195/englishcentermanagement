using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.Courses;

public class EnrollModel : PageModel
{
    private readonly IApiClient _apiClient;

    public EnrollModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(long courseId)
    {
        if (courseId <= 0)
        {
            TempData["ErrorMessage"] = "Khóa học không hợp lệ.";
            return RedirectToPage("/Courses/Index");
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
            return RedirectToPage("/Courses/Index");
        }

        if (!me.StudentId.HasValue || me.StudentId.Value <= 0)
        {
            TempData["ErrorMessage"] = "Bạn chưa có hồ sơ học viên. Vui lòng cập nhật hồ sơ trước.";
            return RedirectToPage("/Student/Profile");
        }

        var profile = await _apiClient.GetAsync<StudentProfileDetailDto>($"students/{me.StudentId.Value}");
        if (!IsProfileCompleted(profile))
        {
            TempData["ErrorMessage"] = "Vui lòng hoàn thiện hồ sơ cá nhân trước khi đăng ký khóa học.";
            return RedirectToPage("/Student/Profile");
        }

        var classesPaged = await _apiClient.GetAsync<PagedResult<ClassDto>>("classes?PageNumber=1&PageSize=500&Status=1");
        var targetClass = classesPaged?.Items?
            .Where(x => x.CourseId == courseId && x.Status == 1)
            .OrderBy(x => x.StartDate)
            .FirstOrDefault();

        if (targetClass == null)
        {
            TempData["ErrorMessage"] = "Hiện chưa có lớp mở cho khóa học này.";
            return RedirectToPage("/Courses/Index");
        }

        var enrollDate = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
        var created = await _apiClient.PostAsync<CreateEnrollmentRequest, object>("enrollments", new CreateEnrollmentRequest
        {
            StudentId = me.StudentId.Value,
            ClassId = targetClass.Id,
            EnrollDate = enrollDate,
            Status = 1,
            Note = "Self-enrolled from course catalog"
        });

        if (created == null)
        {
            TempData["ErrorMessage"] = "Đăng ký khóa học thất bại. Vui lòng thử lại.";
            return RedirectToPage("/Courses/Index");
        }

        HttpContext.Session.SetString("HasAnyEnrollment", "true");
        TempData["SuccessMessage"] = "Đăng ký khóa học thành công.";
        return RedirectToPage("/Student/Index");
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
