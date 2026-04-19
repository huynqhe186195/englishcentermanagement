namespace EnglishCenter.Web.Models;

public class StudentAcademicSummaryDto
{
    public long StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Status { get; set; }

    public int ActiveEnrollments { get; set; }
    public int SuspendedEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public int TransferredEnrollments { get; set; }
    public int CancelledEnrollments { get; set; }

    public int TotalAttendanceRecords { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public decimal AttendanceRate { get; set; }

    public int UpcomingSessions { get; set; }
}
