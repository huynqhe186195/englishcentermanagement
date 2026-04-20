namespace EnglishCenter.Web.Models;

public class ClassDashboardDto
{
    public long ClassId { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public int ActiveEnrollments { get; set; }
    public int SuspendedEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public int TotalSessions { get; set; }
    public int UpcomingSessions { get; set; }
    public decimal AttendanceRate { get; set; }
}

public class TeacherWorkloadDto
{
    public long TeacherId { get; set; }
    public string TeacherCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int TotalAssignedClasses { get; set; }
    public int TotalSessions { get; set; }
    public int UpcomingSessions { get; set; }
    public int TodaySessions { get; set; }
}

public class StudentAtRiskDto
{
    public long StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public decimal AttendanceRate { get; set; }
    public int SuspendedEnrollments { get; set; }
}

public class RoomUtilizationDto
{
    public long RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int TotalSessions { get; set; }
    public int UpcomingSessions { get; set; }
}
