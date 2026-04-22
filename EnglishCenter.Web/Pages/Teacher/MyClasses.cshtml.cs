using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.Teacher;

public class MyClassesModel : PageModel
{
    private readonly IApiClient _apiClient;

    public MyClassesModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty(SupportsGet = true)] public long? ClassId { get; set; }

    public long? TeacherId { get; set; }
    public List<long> ClassIds { get; set; } = new();
    public ClassSummaryDto? SelectedSummary { get; set; }
    public List<ClassRosterItemDto> Roster { get; set; } = new();

    public async Task OnGetAsync()
    {
        TeacherId = (await _apiClient.GetAsync<CurrentUserDto>("auth/me"))?.TeacherId;
        if (!TeacherId.HasValue) return;

        var timetable = await _apiClient.GetAsync<PagedResult<TimetableItemDto>>(
            $"teachers/{TeacherId.Value}/timetable?PageNumber=1&PageSize=500&SortBy=SessionDate&SortDirection=asc");

        ClassIds = timetable?.Items?
            .Select(x => x.ClassId)
            .Distinct()
            .OrderBy(x => x)
            .ToList() ?? new List<long>();

        if (!ClassId.HasValue && ClassIds.Any())
        {
            ClassId = ClassIds.First();
        }

        if (!ClassId.HasValue) return;

        SelectedSummary = await _apiClient.GetAsync<ClassSummaryDto>($"classes/{ClassId.Value}/summary");
        Roster = await _apiClient.GetAsync<List<ClassRosterItemDto>>($"classes/{ClassId.Value}/roster") ?? new List<ClassRosterItemDto>();
    }
}
