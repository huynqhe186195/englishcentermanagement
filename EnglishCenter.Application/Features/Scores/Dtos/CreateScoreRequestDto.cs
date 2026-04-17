namespace EnglishCenter.Application.Features.Scores.Dtos;

public class CreateScoreRequestDto
{
    public long ExamId { get; set; }
    public long StudentId { get; set; }
    public decimal ScoreValue { get; set; }
    public string? Remark { get; set; }
}
