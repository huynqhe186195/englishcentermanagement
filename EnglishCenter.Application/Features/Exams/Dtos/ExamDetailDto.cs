namespace EnglishCenter.Application.Features.Exams.Dtos;

public class ExamDetailDto
{
    public long Id { get; set; }
    public long ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int ExamType { get; set; }
    public DateTime ExamDate { get; set; }
    public decimal MaxScore { get; set; }
    public string? Description { get; set; }
    public long? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
