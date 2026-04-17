namespace EnglishCenter.Application.Features.Assignments.Dtos;

public class AssignmentDto
{
    public long Id { get; set; }
    public long ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public decimal? MaxScore { get; set; }
}
