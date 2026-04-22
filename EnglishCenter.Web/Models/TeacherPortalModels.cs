namespace EnglishCenter.Web.Models;

public class TeacherSummaryDto
{
    public long TeacherId { get; set; }
    public string TeacherCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Status { get; set; }
    public int TotalAssignedClasses { get; set; }
    public int TotalSessions { get; set; }
    public int PlannedSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int CancelledSessions { get; set; }
    public int UpcomingSessions { get; set; }
    public int TodaySessions { get; set; }
}

public class ClassSummaryDto
{
    public long ClassId { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public int ActiveEnrollments { get; set; }
    public int SuspendedEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public int TransferredEnrollments { get; set; }
    public int CancelledEnrollments { get; set; }
    public int TotalSessions { get; set; }
    public int PlannedSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int CancelledSessions { get; set; }
    public decimal AttendanceRate { get; set; }
}

public class SessionAttendanceRosterItemDto
{
    public long StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public long EnrollmentId { get; set; }
    public int EnrollmentStatus { get; set; }
    public int? AttendanceStatus { get; set; }
    public string? Note { get; set; }
    public DateTime? CheckedAt { get; set; }
}

public class MarkAttendanceRequest
{
    public long SessionId { get; set; }
    public List<MarkAttendanceItemRequest> Items { get; set; } = new();
}

public class MarkAttendanceItemRequest
{
    public long StudentId { get; set; }
    public int Status { get; set; }
    public string? Note { get; set; }
}
