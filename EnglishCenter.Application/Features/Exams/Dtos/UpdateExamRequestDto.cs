namespace EnglishCenter.Application.Features.Exams.Dtos;

public class UpdateExamRequestDto
{
    public long ClassId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ExamType { get; set; }
    public DateTime ExamDate { get; set; }
    public decimal MaxScore { get; set; }
    public string? Description { get; set; }
}
