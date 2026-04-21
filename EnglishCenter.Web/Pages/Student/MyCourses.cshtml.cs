using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Student;

public class MyCoursesModel : PageModel
{
    private readonly IApiClient _apiClient;

    public MyCoursesModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public List<EnrollmentDto> Courses { get; set; } = new();

    public int ActiveCount => Courses.Count(x => x.Status == 1);
    public int CompletedCount => Courses.Count(x => x.Status != 1);
    public int TotalCount => Courses.Count;

    public async Task OnGetAsync()
    {
        var me = await _apiClient.GetAsync<CurrentUserDto>("auth/me");
        if (me != null)
        {
            UserName = me.UserName;
            FullName = me.FullName;
        }

        var enrollments = await _apiClient.GetAsync<PagedResult<EnrollmentDto>>("enrollments?PageNumber=1&PageSize=10");
        Courses = enrollments?.Items?.ToList() ?? new List<EnrollmentDto>();
    }
}
