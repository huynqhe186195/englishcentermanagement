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

    public async Task OnGetAsync()
    {
        TeacherId = await ResolveTeacherIdAsync();
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

    private async Task<long?> ResolveTeacherIdAsync()
    {
        var me = await _apiClient.GetAsync<CurrentUserDto>("auth/me");
        return me?.TeacherId;
    }
}
