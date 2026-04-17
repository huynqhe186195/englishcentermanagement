namespace EnglishCenter.Application.Features.Teachers.Dtos;

public class UpdateTeacherRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Specialization { get; set; }
    public string? Qualification { get; set; }
    public DateOnly? HireDate { get; set; }
    public int Status { get; set; }
}
