namespace EnglishCenter.Web.Models;

public class AvailableSlotDto
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int ConflictingStudentCount { get; set; }
    public List<ConflictingStudentDto> ConflictingStudents { get; set; } = new();
}
