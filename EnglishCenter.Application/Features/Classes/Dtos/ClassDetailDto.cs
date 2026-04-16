namespace EnglishCenter.Application.Features.Classes.Dtos;

public class ClassDetailDto
{
    public long Id { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public long CourseId { get; set; }
    public long? CampusId { get; set; }
    public long? RoomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int MaxStudents { get; set; }
    public decimal TuitionFee { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
