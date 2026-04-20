using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Teacher;

public class IndexModel : PageModel
{
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IEnumerable<TeacherWorkloadDto> Workloads { get; set; } = Enumerable.Empty<TeacherWorkloadDto>();

    public async Task OnGetAsync()
    {
        var data = await _apiClient.GetAsync<PagedResult<TeacherWorkloadDto>>("academicDashboard/teacher-workload?PageNumber=1&PageSize=10");
        if (data != null) Workloads = data.Items;
    }
}
