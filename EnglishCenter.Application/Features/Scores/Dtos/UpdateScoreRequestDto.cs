namespace EnglishCenter.Application.Features.Scores.Dtos;

public class UpdateScoreRequestDto
{
    public decimal ScoreValue { get; set; }
    public string? Remark { get; set; }
}
