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

    public List<EnrollmentDto> Enrollments { get; set; } = new();
    public StudentAttendanceReportDto Report { get; set; } = new();
    public List<CalendarDayVm> CalendarDays { get; set; } = new();

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

        CalendarDays = BuildCalendar(Report.Sessions);
    }

    private static List<CalendarDayVm> BuildCalendar(List<StudentAttendanceReportSessionItemDto> sessions)
    {
        var current = sessions.Any()
            ? sessions[0].SessionDate.ToDateTime(TimeOnly.MinValue)
            : DateTime.UtcNow;

        var monthStart = new DateTime(current.Year, current.Month, 1);
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
}
