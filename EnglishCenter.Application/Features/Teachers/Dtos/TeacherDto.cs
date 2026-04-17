namespace EnglishCenter.Application.Features.Teachers.Dtos;

public class TeacherDto
{
    public long Id { get; set; }
    public string TeacherCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Specialization { get; set; }
    public string? Qualification { get; set; }
    public int Status { get; set; }
}
