namespace EnglishCenter.Web.Models;

public class TimetableItemDto
{
    public long SessionId { get; set; }
    public long ClassId { get; set; }
    public int SessionNo { get; set; }
    public string SessionDate { get; set; } = string.Empty; // yyyy-MM-dd
    public string StartTime { get; set; } = string.Empty; // HH:mm
    public string EndTime { get; set; } = string.Empty;
    public long? TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public long? RoomId { get; set; }
    public string? Topic { get; set; }
    public string? Note { get; set; }
    public int Status { get; set; }
}
