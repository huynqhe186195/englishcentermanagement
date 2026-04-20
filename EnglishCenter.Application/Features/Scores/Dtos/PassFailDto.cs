namespace EnglishCenter.Application.Features.Scores.Dtos;

public class PassFailDto
{
    public long StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public decimal AverageScore { get; set; }
    public int PresentCount { get; set; }
    public int TotalSessions { get; set; }
    public decimal AttendancePercent { get; set; }
    public bool IsPassed { get; set; }
}
