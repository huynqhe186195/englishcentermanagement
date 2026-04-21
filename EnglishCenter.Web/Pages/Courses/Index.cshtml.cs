using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;

namespace EnglishCenter.Web.Pages.Courses;

public class IndexModel : PageModel
{
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<CourseDto> Courses { get; set; } = new();

    public async Task OnGetAsync()
    {
        var pagedCourses = await _apiClient.GetAsync<PagedResult<CourseDto>>(
            "courses?PageNumber=1&PageSize=12&SortBy=CreatedAt&SortDirection=desc");

        Courses = pagedCourses?.Items
            .Where(x => x.Status == 1)
            .ToList() ?? new List<CourseDto>();
    }
}
