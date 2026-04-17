namespace EnglishCenter.Application.Features.Attendance.Dtos;

public class MarkAttendanceRequestDto
{
    public long SessionId { get; set; }
    public List<AttendanceItemDto> Items { get; set; } = [];
}