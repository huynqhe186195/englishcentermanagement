namespace EnglishCenter.Application.Features.Notifications.Dtos;

public class CreateNotificationRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Channel { get; set; }
    public int TargetType { get; set; }
    public long? TargetId { get; set; }
    public long? CreatedByUserId { get; set; }
}
