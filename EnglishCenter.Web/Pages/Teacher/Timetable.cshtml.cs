using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.Teacher;

public class TimetableModel : PageModel
{
    private readonly IApiClient _apiClient;

    public TimetableModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty(SupportsGet = true)] public string? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public string? ToDate { get; set; }

    public List<TimetableItemDto> Sessions { get; set; } = new();
    public long? TeacherId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        var me = await _apiClient.GetAsync<CurrentUserDto>("auth/me");
        TeacherId = me?.TeacherId;
        FullName = me?.FullName ?? string.Empty;
        UserName = me?.UserName ?? string.Empty;
        if (!TeacherId.HasValue) return;

        var from = string.IsNullOrWhiteSpace(FromDate) ? DateTime.Today.ToString("yyyy-MM-dd") : FromDate;
        var to = string.IsNullOrWhiteSpace(ToDate) ? DateTime.Today.AddDays(14).ToString("yyyy-MM-dd") : ToDate;

        var result = await _apiClient.GetAsync<PagedResult<TimetableItemDto>>(
            $"teachers/{TeacherId.Value}/timetable?PageNumber=1&PageSize=200&FromDate={from}&ToDate={to}&SortBy=SessionDate&SortDirection=asc");

        Sessions = result?.Items?.OrderBy(x => x.SessionDate).ThenBy(x => x.StartTime).ToList() ?? new List<TimetableItemDto>();
        FromDate = from;
        ToDate = to;
    }
}
