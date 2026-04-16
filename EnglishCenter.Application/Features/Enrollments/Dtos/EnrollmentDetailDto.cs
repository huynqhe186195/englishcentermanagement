namespace EnglishCenter.Application.Features.Enrollments.Dtos;

public class EnrollmentDetailDto
{
    public long Id { get; set; }
    public long StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public long ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateOnly EnrollDate { get; set; }
    public string? Note { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
