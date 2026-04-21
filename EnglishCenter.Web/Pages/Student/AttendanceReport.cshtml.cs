using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using System.Globalization;

namespace EnglishCenter.Web.Pages.Student;

public class AttendanceReportModel : PageModel
{
    private readonly IApiClient _apiClient;

    public AttendanceReportModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public long? ClassId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Month { get; set; }

    public List<EnrollmentDto> Enrollments { get; set; } = new();
    public StudentAttendanceReportDto Report { get; set; } = new();
    public List<CalendarDayVm> CalendarDays { get; set; } = new();
    public List<MonthOptionVm> MonthOptions { get; set; } = new();
    public string DisplayMonthLabel { get; set; } = DateTime.UtcNow.ToString("MM/yyyy");

    public int ExcusedAbsentCount => Report.Sessions.Count(x => (x.AttendanceStatusText ?? string.Empty).Contains("Có phép", StringComparison.OrdinalIgnoreCase));
    public int UnexcusedAbsentCount => Math.Max(Report.AbsentCount - ExcusedAbsentCount, 0);

    public string AttendanceRateText => (100 - Report.AbsentRate).ToString("0.#");

    public async Task OnGetAsync()
    {
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
        ClassId ??= Enrollments.FirstOrDefault()?.ClassId;

        if (studentId > 0 && ClassId.HasValue)
        {
            var url = $"students/{studentId}/attendance-report?ClassId={ClassId.Value}&SendWarningEmail=false";
            Report = await _apiClient.GetAsync<StudentAttendanceReportDto>(url) ?? new StudentAttendanceReportDto();
        }

        var displayMonth = ResolveDisplayMonth(Report.Sessions, Month);
        Month = displayMonth.ToString("yyyy-MM");
        DisplayMonthLabel = displayMonth.ToString("MM/yyyy");
        MonthOptions = BuildMonthOptions(Report.Sessions, displayMonth);
        CalendarDays = BuildCalendar(Report.Sessions, displayMonth);
    }

    private static DateTime ResolveDisplayMonth(List<StudentAttendanceReportSessionItemDto> sessions, string? requestedMonth)
    {
        if (!string.IsNullOrWhiteSpace(requestedMonth)
            && DateTime.TryParseExact(requestedMonth + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        if (sessions.Any())
        {
            var latestSessionDate = sessions.Max(x => x.SessionDate);
            return latestSessionDate.ToDateTime(TimeOnly.MinValue);
        }

        return DateTime.UtcNow;
    }

    private static List<MonthOptionVm> BuildMonthOptions(List<StudentAttendanceReportSessionItemDto> sessions, DateTime displayMonth)
    {
        var options = sessions
            .Select(x => new DateTime(x.SessionDate.Year, x.SessionDate.Month, 1))
            .Distinct()
            .OrderByDescending(x => x)
            .Select(x => new MonthOptionVm
            {
                Value = x.ToString("yyyy-MM"),
                Label = x.ToString("MM/yyyy")
            })
            .ToList();

        var displayMonthValue = displayMonth.ToString("yyyy-MM");
        if (!options.Any(x => x.Value == displayMonthValue))
        {
            options.Insert(0, new MonthOptionVm
            {
                Value = displayMonthValue,
                Label = displayMonth.ToString("MM/yyyy")
            });
        }

        return options;
    }

    private static List<CalendarDayVm> BuildCalendar(List<StudentAttendanceReportSessionItemDto> sessions, DateTime displayMonth)
    {
        var monthStart = new DateTime(displayMonth.Year, displayMonth.Month, 1);
        var startOffset = ((int)monthStart.DayOfWeek + 6) % 7;
        var calendarStart = monthStart.AddDays(-startOffset);

        var map = sessions
            .GroupBy(x => x.SessionDate)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.StartTime).ToList());

        var result = new List<CalendarDayVm>();
        for (var i = 0; i < 35; i++)
        {
            var day = DateOnly.FromDateTime(calendarStart.AddDays(i));
            map.TryGetValue(day, out var daySessions);

            result.Add(new CalendarDayVm
            {
                Date = day,
                IsCurrentMonth = day.Month == monthStart.Month,
                Sessions = daySessions ?? new List<StudentAttendanceReportSessionItemDto>()
            });
        }

        return result;
    }

    public class CalendarDayVm
    {
        public DateOnly Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public List<StudentAttendanceReportSessionItemDto> Sessions { get; set; } = new();
    }

    public class MonthOptionVm
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
