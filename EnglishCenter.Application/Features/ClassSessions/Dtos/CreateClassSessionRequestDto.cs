namespace EnglishCenter.Application.Features.ClassSessions.Dtos;

public class CreateClassSessionRequestDto
{
    public long ClassId { get; set; }
    public int SessionNo { get; set; }
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public long? RoomId { get; set; }
    public long? TeacherId { get; set; }
    public string? Topic { get; set; }
    public string? Note { get; set; }
    public int Status { get; set; } = 1;
}