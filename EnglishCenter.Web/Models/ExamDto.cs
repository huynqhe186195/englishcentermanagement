namespace EnglishCenter.Web.Models;

public class ExamDto
{
    public long Id { get; set; }
    public long ClassId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ExamType { get; set; }
    public DateTime ExamDate { get; set; }
    public decimal MaxScore { get; set; }
    public string? Description { get; set; }
    public int Status { get; set; }
}
