using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Student;

public class MyCoursesModel : PageModel
{
    private readonly IApiClient _apiClient;

    public MyCoursesModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public List<EnrollmentDto> Courses { get; set; } = new();
    public List<StudentCourseCardVm> CardCourses { get; set; } = new();

    public int ActiveCount => Courses.Count(x => x.Status == 1);
    public int CompletedCount => Courses.Count(x => x.Status != 1);
    public int TotalCount => Courses.Count;

    public string TotalHoursLabel
    {
        get
        {
            var studiedHours = CardCourses.Sum(x => x.StudiedHours);
            var totalHours = CardCourses.Sum(x => x.TotalHours);
            return $"{studiedHours}/{Math.Max(totalHours, 1)}";
        }
    }

    public async Task OnGetAsync()
    {
        var me = await _apiClient.GetAsync<CurrentUserDto>("auth/me");
        if (me != null)
        {
            UserName = me.UserName;
            FullName = me.FullName;
        }

        var enrollments = await _apiClient.GetAsync<PagedResult<EnrollmentDto>>("enrollments?PageNumber=1&PageSize=100");
        var allEnrollments = enrollments?.Items?.ToList() ?? new List<EnrollmentDto>();

        Courses = allEnrollments
            .Where(x => (me?.StudentId.HasValue == true && x.StudentId == me.StudentId.Value)
                || (!string.IsNullOrWhiteSpace(FullName)
                    && (x.StudentName.Equals(FullName, StringComparison.OrdinalIgnoreCase)
                        || x.StudentName.Contains(FullName, StringComparison.OrdinalIgnoreCase))))
            .ToList();

        if (!Courses.Any())
        {
            Courses = allEnrollments;
        }

        var cards = new List<StudentCourseCardVm>();
        foreach (var enrollment in Courses)
        {
            cards.Add(await BuildCardAsync(enrollment, me?.StudentId ?? enrollment.StudentId));
        }

        CardCourses = cards;
    }

    private async Task<StudentCourseCardVm> BuildCardAsync(EnrollmentDto enrollment, long studentId)
    {
        var report = await _apiClient.GetAsync<StudentAttendanceReportDto>(
            $"students/{studentId}/attendance-report?ClassId={enrollment.ClassId}&SendWarningEmail=false");

        var timetable = await _apiClient.GetAsync<PagedResult<TimetableItemDto>>(
            $"classes/{enrollment.ClassId}/timetable?PageNumber=1&PageSize=100&SortBy=SessionDate&SortDirection=asc");

        var sessions = timetable?.Items?.ToList() ?? new List<TimetableItemDto>();
        var now = DateTime.UtcNow;
        var nextSession = sessions
            .Where(x => DateTime.TryParse($"{x.SessionDate} {x.StartTime}", out var dt) && dt >= now)
            .OrderBy(x => x.SessionDate)
            .ThenBy(x => x.StartTime)
            .FirstOrDefault();

        var totalSessions = report?.TotalValidSessions ?? sessions.Count;
        var presentCount = report?.PresentCount ?? 0;
        var progressPercent = totalSessions > 0
            ? (int)Math.Round((presentCount * 100.0) / totalSessions)
            : 0;

        var absentCount = report?.AbsentCount ?? 0;
        var statusText = enrollment.Status == 1 ? "Đang học" : "Đã hoàn thành";

        return new StudentCourseCardVm
        {
            EnrollmentId = enrollment.Id,
            CourseName = enrollment.ClassName,
            StatusText = statusText,
            ClassId = enrollment.ClassId,
            ProgressPercent = Math.Clamp(progressPercent, 0, 100),
            CompletedSessions = presentCount,
            TotalSessions = totalSessions,
            NextSessionText = nextSession != null
                ? $"{nextSession.SessionDate} {nextSession.StartTime}-{nextSession.EndTime}"
                : "Chưa có lịch từ API",
            RoomText = nextSession?.RoomId?.ToString() ?? "N/A",
            StudiedHours = presentCount * 2,
            TotalHours = Math.Max(totalSessions * 2, 2),
            AttendanceSummary = $"{presentCount} có mặt • {absentCount} vắng"
        };
    }

    public class StudentCourseCardVm
    {
        public long EnrollmentId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public long ClassId { get; set; }
        public int ProgressPercent { get; set; }
        public int CompletedSessions { get; set; }
        public int TotalSessions { get; set; }
        public string NextSessionText { get; set; } = string.Empty;
        public string RoomText { get; set; } = string.Empty;
        public int StudiedHours { get; set; }
        public int TotalHours { get; set; }
        public string AttendanceSummary { get; set; } = string.Empty;
    }
}
