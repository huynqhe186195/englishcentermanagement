namespace EnglishCenter.Application.Features.Notifications.Dtos;

public class NotificationDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Channel { get; set; }
    public int TargetType { get; set; }
    public long? TargetId { get; set; }
    public int Status { get; set; }
    public DateTime? SentAt { get; set; }
}
