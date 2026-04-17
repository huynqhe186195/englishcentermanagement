namespace EnglishCenter.Application.Features.ClassSchedules.Dtos;

public class UpdateClassScheduleRequestDto
{
    public int DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public long? RoomId { get; set; }
}