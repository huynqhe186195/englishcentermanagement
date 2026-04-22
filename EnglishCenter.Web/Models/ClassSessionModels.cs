namespace EnglishCenter.Web.Models;

public class ClassSessionDto
{
    public long Id { get; set; }
    public long ClassId { get; set; }
    public int SessionNo { get; set; }
    public string SessionDate { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public long? RoomId { get; set; }
    public long? TeacherId { get; set; }
    public string? Topic { get; set; }
    public string? Note { get; set; }
    public int Status { get; set; }
}

public class CreateClassSessionRequest
{
    public long ClassId { get; set; }
    public int SessionNo { get; set; }
    public string SessionDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string StartTime { get; set; } = "18:00";
    public string EndTime { get; set; } = "19:30";
    public long? RoomId { get; set; }
    public long? TeacherId { get; set; }
    public string? Topic { get; set; }
    public string? Note { get; set; }
    public int Status { get; set; } = 1;
}
