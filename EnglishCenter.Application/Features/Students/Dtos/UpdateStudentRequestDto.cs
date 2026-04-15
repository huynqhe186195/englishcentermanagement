namespace EnglishCenter.Application.Features.Students.Dtos;

public class UpdateStudentRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public int? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? SchoolName { get; set; }
    public string? EnglishLevel { get; set; }
    public string? Note { get; set; }
    public int Status { get; set; }
}