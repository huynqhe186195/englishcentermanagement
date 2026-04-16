namespace EnglishCenter.Application.Features.Enrollments.Dtos;

public class CreateEnrollmentRequestDto
{
    public long StudentId { get; set; }
    public long ClassId { get; set; }
    public DateOnly EnrollDate { get; set; }
    public string? Note { get; set; }
    public int Status { get; set; } = 1;
}
