namespace EnglishCenter.Application.Features.Scores.Dtos;

public class ImportScoreItemDto
{
    public long? StudentId { get; set; }
    public string? StudentCode { get; set; }
    public decimal ScoreValue { get; set; }
    public string? Remark { get; set; }
}
