namespace EnglishCenter.Application.Features.Attendance.Dtos;

public class AttendanceItemDto
{
    public long StudentId { get; set; }
    public int Status { get; set; }
    public string? Note { get; set; }
}