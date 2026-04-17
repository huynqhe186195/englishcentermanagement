namespace EnglishCenter.Application.Features.Exams.Dtos;

public class ExamDto
{
    public long Id { get; set; }
    public long ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int ExamType { get; set; }
    public DateTime ExamDate { get; set; }
    public decimal MaxScore { get; set; }
    public int Status { get; set; }
}
