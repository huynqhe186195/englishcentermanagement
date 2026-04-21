namespace EnglishCenter.Web.Models;

public class ScoreDto
{
    public long Id { get; set; }
    public long ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public long StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public decimal ScoreValue { get; set; }
    public string? Remark { get; set; }
}
