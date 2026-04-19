namespace EnglishCenter.Web.Models;

public class ClassRosterItemDto
{
    public long EnrollmentId { get; set; }
    public long StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int EnrollmentStatus { get; set; }
    public string EnrollDate { get; set; } = string.Empty; // yyyy-MM-dd
}
