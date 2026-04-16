namespace EnglishCenter.Application.Features.Enrollments.Dtos;

public class EnrollmentDto
{
    public long Id { get; set; }
    public long StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public long ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateOnly EnrollDate { get; set; }
    public int Status { get; set; }
}
