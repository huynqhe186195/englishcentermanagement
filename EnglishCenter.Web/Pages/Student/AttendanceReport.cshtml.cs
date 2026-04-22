using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
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
    public int? Year { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? WeekStart { get; set; }

    public List<EnrollmentDto> Enrollments { get; set; } = new();
    public StudentAttendanceReportDto Report { get; set; } = new();
    public List<CalendarDayVm> CalendarDays { get; set; } = new();
    public List<int> YearOptions { get; set; } = new();
    public List<WeekOptionVm> WeekOptions { get; set; } = new();
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

        var selectedYear = ResolveSelectedYear(Report.Sessions, Year);
        Year = selectedYear;
        YearOptions = BuildYearOptions(Report.Sessions, selectedYear);

        WeekOptions = BuildWeekOptions(Report.Sessions, selectedYear);
        var selectedWeekStart = ResolveSelectedWeekStart(WeekOptions, WeekStart, selectedYear);
        WeekStart = selectedWeekStart.ToString("yyyy-MM-dd");
        var displayMonth = new DateTime(selectedWeekStart.Year, selectedWeekStart.Month, 1);
        DisplayMonthLabel = displayMonth.ToString("MM/yyyy");
        CalendarDays = BuildCalendar(Report.Sessions, displayMonth);
    }

    private static int ResolveSelectedYear(List<StudentAttendanceReportSessionItemDto> sessions, int? requestedYear)
    {
        if (requestedYear.HasValue)
        {
            return requestedYear.Value;
        }

        if (sessions.Any())
        {
            return sessions.Max(x => x.SessionDate.Year);
        }

        return DateTime.UtcNow.Year;
    }

    private static List<int> BuildYearOptions(List<StudentAttendanceReportSessionItemDto> sessions, int selectedYear)
    {
        var options = sessions
            .Select(x => x.SessionDate.Year)
            .Distinct()
            .OrderByDescending(x => x)
            .ToList();

        if (!options.Contains(selectedYear))
        {
            options.Insert(0, selectedYear);
        }

        return options;
    }

    private static List<WeekOptionVm> BuildWeekOptions(List<StudentAttendanceReportSessionItemDto> sessions, int selectedYear)
    {
        var weekStarts = sessions
            .Where(x => x.SessionDate.Year == selectedYear)
            .Select(x => GetWeekStart(x.SessionDate.ToDateTime(TimeOnly.MinValue)))
            .Distinct()
            .OrderByDescending(x => x)
            .ToList();

        return weekStarts
            .Select(x => new WeekOptionVm
            {
                Value = x.ToString("yyyy-MM-dd"),
                Label = $"{x:dd/MM} đến {x.AddDays(6):dd/MM}"
            })
            .ToList();
    }

    private static DateTime ResolveSelectedWeekStart(List<WeekOptionVm> weekOptions, string? requestedWeekStart, int selectedYear)
    {
        if (!string.IsNullOrWhiteSpace(requestedWeekStart)
            && DateTime.TryParse(requestedWeekStart, out var parsedWeek))
        {
            return parsedWeek.Date;
        }

        if (weekOptions.Any() && DateTime.TryParse(weekOptions[0].Value, out var firstWeek))
        {
            return firstWeek.Date;
        }

        var firstDay = new DateTime(selectedYear, 1, 1);
        return GetWeekStart(firstDay);
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

    private static DateTime GetWeekStart(DateTime date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-offset);
    }

    public class CalendarDayVm
    {
        public DateOnly Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public List<StudentAttendanceReportSessionItemDto> Sessions { get; set; } = new();
    }

    public class WeekOptionVm
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
