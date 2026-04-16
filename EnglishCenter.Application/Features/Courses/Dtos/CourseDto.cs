namespace EnglishCenter.Application.Features.Courses.Dtos;

public class CourseDto
{
    public long Id { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Level { get; set; }
    public int? AgeMin { get; set; }
    public int? AgeMax { get; set; }
    public int TotalSessions { get; set; }
    public decimal DefaultFee { get; set; }
    public int Status { get; set; }
}