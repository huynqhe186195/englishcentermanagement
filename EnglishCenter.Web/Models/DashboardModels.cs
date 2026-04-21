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

public class RevenueSummaryDto
{
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int UnpaidInvoices { get; set; }
    public int CancelledInvoices { get; set; }
    public decimal TotalExpectedRevenue { get; set; }
    public decimal TotalCollectedRevenue { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalRefundedAmount { get; set; }
}

public class RevenueByCampusItemDto
{
    public long CampusId { get; set; }
    public string CampusCode { get; set; } = string.Empty;
    public string CampusName { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public int PaidInvoiceCount { get; set; }
    public int UnpaidInvoiceCount { get; set; }
    public int CancelledInvoiceCount { get; set; }
    public decimal ExpectedRevenue { get; set; }
    public decimal CollectedRevenue { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal RefundedAmount { get; set; }
}

public class ClassDashboardByCampusItemDto
{
    public long CampusId { get; set; }
    public string CampusCode { get; set; } = string.Empty;
    public string CampusName { get; set; } = string.Empty;
    public int ClassCount { get; set; }
    public int ActiveClassCount { get; set; }
    public int ActiveEnrollments { get; set; }
    public int SuspendedEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public int TransferredEnrollments { get; set; }
    public int CancelledEnrollments { get; set; }
    public int TotalSessions { get; set; }
    public int PlannedSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int CancelledSessions { get; set; }
    public int UpcomingSessions { get; set; }
    public decimal AttendanceRate { get; set; }
}

public class TeacherWorkloadByCampusItemDto
{
    public long CampusId { get; set; }
    public string CampusCode { get; set; } = string.Empty;
    public string CampusName { get; set; } = string.Empty;
    public int TeacherCount { get; set; }
    public int ActiveTeacherCount { get; set; }
    public int TotalAssignedClasses { get; set; }
    public int TotalSessions { get; set; }
    public int PlannedSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int CancelledSessions { get; set; }
    public int UpcomingSessions { get; set; }
    public int TodaySessions { get; set; }
}

public class RoomUtilizationByCampusItemDto
{
    public long CampusId { get; set; }
    public string CampusCode { get; set; } = string.Empty;
    public string CampusName { get; set; } = string.Empty;
    public int RoomCount { get; set; }
    public int ActiveRoomCount { get; set; }
    public int TotalAssignedClasses { get; set; }
    public int TotalSessions { get; set; }
    public int PlannedSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int CancelledSessions { get; set; }
    public int UpcomingSessions { get; set; }
    public int TodaySessions { get; set; }
}
