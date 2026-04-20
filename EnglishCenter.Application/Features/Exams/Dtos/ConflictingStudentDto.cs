namespace EnglishCenter.Application.Features.Exams.Dtos;

public class ConflictingStudentDto
{
    public long StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
}
