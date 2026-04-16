namespace EnglishCenter.Application.Features.Assignments.Dtos;

public class CreateAssignmentRequestDto
{
    public long ClassId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? MaxScore { get; set; }
    public long? CreatedByUserId { get; set; }
}
