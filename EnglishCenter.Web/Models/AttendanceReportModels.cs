namespace EnglishCenter.Web.Models;

public class StudentAttendanceReportDto
{
    public long StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public long EnrollmentId { get; set; }
    public int EnrollmentStatus { get; set; }
    public long ClassId { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int TotalValidSessions { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int NotMarkedCount { get; set; }
    public decimal AbsentRate { get; set; }
    public bool IsWarning { get; set; }
    public string? WarningMessage { get; set; }
    public List<StudentAttendanceReportSessionItemDto> Sessions { get; set; } = new();
}

public class StudentAttendanceReportSessionItemDto
{
    public long SessionId { get; set; }
    public int SessionNo { get; set; }
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SessionStatus { get; set; }
    public string SessionStatusText { get; set; } = string.Empty;
    public int? AttendanceStatus { get; set; }
    public string AttendanceStatusText { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime? CheckedAt { get; set; }
}
