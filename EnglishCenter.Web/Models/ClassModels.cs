namespace EnglishCenter.Web.Models;

public class ClassDto
{
    public long Id { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public long CourseId { get; set; }
    public long? CampusId { get; set; }
    public long? RoomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty; // yyyy-MM-dd
    public string EndDate { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public decimal TuitionFee { get; set; }
    public int Status { get; set; }
}

public class ClassDetailDto : ClassDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateClassRequest
{
    public string ClassCode { get; set; } = string.Empty;
    public long CourseId { get; set; }
    public long? CampusId { get; set; }
    public long? RoomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public decimal TuitionFee { get; set; }
    public int Status { get; set; } = 1;
}

public class UpdateClassRequest
{
    public long Id { get; set; }
    public long CourseId { get; set; }
    public long? CampusId { get; set; }
    public long? RoomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public decimal TuitionFee { get; set; }
    public int Status { get; set; }
}
