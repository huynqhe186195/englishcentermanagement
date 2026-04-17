namespace EnglishCenter.Application.Features.ClassSchedules.Dtos;

public class CreateClassScheduleRequestDto
{
    public long ClassId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public long? RoomId { get; set; }
}