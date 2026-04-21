using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;

namespace EnglishCenter.Web.Pages.Student;

public class ScheduleModel : PageModel
{
    private readonly IApiClient _apiClient;

    public ScheduleModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)] public string? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public string? ToDate { get; set; }

    public List<EnrollmentDto> Enrollments { get; set; } = new();
    public List<TimetableItemDto> Items { get; set; } = new();
    public List<DateOnly> WeekDays { get; set; } = new();

    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }

    public async Task OnGetAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));

        WeekStart = DateOnly.TryParse(FromDate, out var from) ? from : monday;
        WeekEnd = DateOnly.TryParse(ToDate, out var to) ? to : WeekStart.AddDays(6);

        WeekDays = Enumerable.Range(0, 7).Select(x => WeekStart.AddDays(x)).ToList();

        var me = await _apiClient.GetAsync<CurrentUserDto>("auth/me");
        if (me != null)
        {
            UserName = me.UserName;
            FullName = me.FullName;
        }

        var enrollmentData = await _apiClient.GetAsync<PagedResult<EnrollmentDto>>("enrollments?PageNumber=1&PageSize=100");
        var allEnrollments = enrollmentData?.Items?.ToList() ?? new List<EnrollmentDto>();

        Enrollments = allEnrollments
            .Where(x => string.IsNullOrWhiteSpace(FullName)
                || x.StudentName.Equals(FullName, StringComparison.OrdinalIgnoreCase)
                || x.StudentName.Contains(FullName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!Enrollments.Any())
        {
            Enrollments = allEnrollments.Take(10).ToList();
        }

        var studentId = Enrollments.FirstOrDefault()?.StudentId ?? 0;
        if (studentId > 0)
        {
            var url = $"students/{studentId}/timetable?PageNumber=1&PageSize=200&FromDate={WeekStart:yyyy-MM-dd}&ToDate={WeekEnd:yyyy-MM-dd}";
            var data = await _apiClient.GetAsync<PagedResult<TimetableItemDto>>(url);
            Items = data?.Items?.OrderBy(x => x.SessionDate).ThenBy(x => x.StartTime).ToList() ?? new List<TimetableItemDto>();
        }
    }

    public List<TimetableItemDto> DayItems(DateOnly day)
        => Items.Where(x => x.SessionDate == day.ToString("yyyy-MM-dd")).ToList();

    public int PositionTop(TimetableItemDto item)
    {
        if (!TimeOnly.TryParse(item.StartTime, out var st)) return 0;
        return Math.Max((st.Hour - 8) * 56 + (st.Minute * 56 / 60), 0);
    }

    public int ItemHeight(TimetableItemDto item)
    {
        if (!TimeOnly.TryParse(item.StartTime, out var st) || !TimeOnly.TryParse(item.EndTime, out var et)) return 56;
        var duration = (int)(et - st).TotalMinutes;
        return Math.Clamp(duration * 56 / 60, 48, 180);
    }
}
