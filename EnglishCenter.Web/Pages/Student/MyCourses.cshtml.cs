using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Student;

public class MyCoursesModel : PageModel
{
    private readonly IApiClient _apiClient;

    private static readonly string[] TeacherPool =
    {
        "GV. Trần Văn Minh",
        "GV. Nguyễn Hoàng Long",
        "GV. Lê Kim Ngân",
        "GV. Phạm Anh Tú"
    };

    private static readonly string[] SchedulePool =
    {
        "Thứ 3 & Thứ 5, 18:00-20:00",
        "Thứ 2 & Thứ 4, 19:00-21:00",
        "Thứ 6 & Chủ nhật, 18:30-20:30",
        "Thứ 7, 08:00-11:00"
    };

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
            var totalHours = Math.Max(TotalCount, 1) * 80;
            var studiedHours = CardCourses.Sum(x => x.StudiedHours);
            return $"{studiedHours}/{totalHours}";
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

        var enrollments = await _apiClient.GetAsync<PagedResult<EnrollmentDto>>("enrollments?PageNumber=1&PageSize=10");
        Courses = enrollments?.Items?.ToList() ?? new List<EnrollmentDto>();
        CardCourses = Courses.Select(ToCardVm).ToList();
    }

    private StudentCourseCardVm ToCardVm(EnrollmentDto enrollment)
    {
        var idx = (int)(enrollment.Id % 4);
        var isActive = enrollment.Status == 1;
        var progressPercent = isActive ? 40 + (int)(enrollment.Id % 30) : 100;
        var totalSessions = 40;
        var completedSessions = (int)Math.Round(totalSessions * (progressPercent / 100.0));

        var listening = Math.Clamp(progressPercent + 5, 40, 100);
        var reading = Math.Clamp(progressPercent - 5, 35, 100);
        var writing = Math.Clamp(progressPercent - 10, 30, 100);
        var speaking = Math.Clamp(progressPercent + 10, 45, 100);

        var nextLesson = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(2 + idx));
        var upcomingTest = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(7 + idx));

        return new StudentCourseCardVm
        {
            EnrollmentId = enrollment.Id,
            CourseName = enrollment.ClassName,
            StatusText = isActive ? "Đang học" : "Đã hoàn thành",
            TeacherName = TeacherPool[idx],
            StudentsText = $"{15 + idx * 5} students",
            RoomText = $"Room {201 + idx}",
            ScheduleText = SchedulePool[idx],
            ProgressPercent = progressPercent,
            CompletedSessions = completedSessions,
            TotalSessions = totalSessions,
            Listening = listening,
            Reading = reading,
            Writing = writing,
            Speaking = speaking,
            NextLessonText = $"Thứ {nextLesson.DayOfWeek switch
            {
                DayOfWeek.Monday => 2,
                DayOfWeek.Tuesday => 3,
                DayOfWeek.Wednesday => 4,
                DayOfWeek.Thursday => 5,
                DayOfWeek.Friday => 6,
                DayOfWeek.Saturday => 7,
                _ => "CN"
            }}, {nextLesson:dd/MM/yyyy} - 18:00",
            UpcomingTestText = $"Mid-term Test - {upcomingTest:dd/MM}",
            StudiedHours = (int)Math.Round((progressPercent / 100.0) * 80)
        };
    }

    public class StudentCourseCardVm
    {
        public long EnrollmentId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string StudentsText { get; set; } = string.Empty;
        public string RoomText { get; set; } = string.Empty;
        public string ScheduleText { get; set; } = string.Empty;
        public int ProgressPercent { get; set; }
        public int CompletedSessions { get; set; }
        public int TotalSessions { get; set; }
        public int Listening { get; set; }
        public int Reading { get; set; }
        public int Writing { get; set; }
        public int Speaking { get; set; }
        public string NextLessonText { get; set; } = string.Empty;
        public string UpcomingTestText { get; set; } = string.Empty;
        public int StudiedHours { get; set; }
    }
}
