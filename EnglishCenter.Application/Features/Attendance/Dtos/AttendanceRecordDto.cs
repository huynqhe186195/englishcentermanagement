namespace EnglishCenter.Application.Features.Attendance.Dtos;

public class AttendanceRecordDto
{
    public long Id { get; set; }
    public long SessionId { get; set; }
    public long StudentId { get; set; }
    public int Status { get; set; }
    public string? Note { get; set; }
    public DateTime CheckedAt { get; set; }
    public long? CheckedByUserId { get; set; }
}