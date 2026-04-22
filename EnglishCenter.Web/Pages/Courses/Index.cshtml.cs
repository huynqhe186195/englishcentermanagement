using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using System.Text.Json;

namespace EnglishCenter.Web.Pages.Courses;

public class IndexModel : PageModel
{
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<CourseDto> Courses { get; set; } = new();
    public bool IsStudentLoggedIn { get; set; }
    public bool HasAnyEnrollment { get; set; }
    public string StudentDisplayName { get; set; } = string.Empty;
    public string ProfilePageUrl => HasAnyEnrollment ? "/Student/Profile" : "/Account/CompleteStudentProfile";
    public string PrimaryCtaText => !IsStudentLoggedIn
        ? "Đăng ký ngay"
        : (HasAnyEnrollment ? "Vào học tập" : "Đăng ký khóa học");

    public async Task OnGetAsync()
    {
        ResolveStudentSessionFlags();

        var pagedCourses = await _apiClient.GetAsync<PagedResult<CourseDto>>(
            "courses?PageNumber=1&PageSize=12&SortBy=CreatedAt&SortDirection=desc");

        Courses = pagedCourses?.Items
            .Where(x => x.Status == 1)
            .ToList() ?? new List<CourseDto>();
    }

    public string GetCtaLink(long? courseId = null)
    {
        if (!IsStudentLoggedIn)
        {
            return "/Account/Register";
        }

        if (HasAnyEnrollment)
        {
            return "/Student/Index";
        }

        return courseId.HasValue
            ? $"/Courses/Details?id={courseId.Value}"
            : "/Courses/Index";
    }

    private void ResolveStudentSessionFlags()
    {
        var rawRoles = HttpContext.Session.GetString("Roles");
        var roles = string.IsNullOrWhiteSpace(rawRoles)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(rawRoles) ?? new List<string>();

        IsStudentLoggedIn = roles.Contains("STUDENT", StringComparer.OrdinalIgnoreCase);
        StudentDisplayName = HttpContext.Session.GetString("FullName")
            ?? HttpContext.Session.GetString("UserName")
            ?? "Học viên";
        HasAnyEnrollment = string.Equals(
            HttpContext.Session.GetString("HasAnyEnrollment"),
            "true",
            StringComparison.OrdinalIgnoreCase);
    }
}
