namespace EnglishCenter.Application.Features.Attendance.Dtos;

public class AttendanceSummaryDto
{
    public long StudentId { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int TotalSessions { get; set; }
}