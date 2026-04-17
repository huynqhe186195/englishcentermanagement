namespace EnglishCenter.Application.Features.ClassSchedules.Dtos;

public class ClassScheduleDto
{
    public long Id { get; set; }
    public long ClassId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public long? RoomId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}