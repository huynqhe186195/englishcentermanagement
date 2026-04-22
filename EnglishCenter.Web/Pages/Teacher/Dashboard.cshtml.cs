using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.Teacher;

public class DashboardModel : PageModel
{
    private readonly IApiClient _apiClient;

    public DashboardModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public TeacherSummaryDto Summary { get; set; } = new();
    public List<TimetableItemDto> TodaySessions { get; set; } = new();
    public long? TeacherId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        var me = await _apiClient.GetAsync<CurrentUserDto>("auth/me");
        TeacherId = me?.TeacherId;
        FullName = me?.FullName ?? string.Empty;
        UserName = me?.UserName ?? string.Empty;

        if (!TeacherId.HasValue)
        {
            return;
        }

        Summary = await _apiClient.GetAsync<TeacherSummaryDto>($"teachers/{TeacherId.Value}/summary") ?? new TeacherSummaryDto();

        var today = DateTime.Today.ToString("yyyy-MM-dd");
        var timetable = await _apiClient.GetAsync<PagedResult<TimetableItemDto>>(
            $"teachers/{TeacherId.Value}/timetable?PageNumber=1&PageSize=50&FromDate={today}&ToDate={today}&SortBy=SessionDate&SortDirection=asc");

        TodaySessions = timetable?.Items?
            .OrderBy(x => x.StartTime)
            .ToList() ?? new List<TimetableItemDto>();
    }
}
