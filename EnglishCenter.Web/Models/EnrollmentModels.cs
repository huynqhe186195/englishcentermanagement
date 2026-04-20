namespace EnglishCenter.Web.Models;

public class EnrollmentDto
{
    public long Id { get; set; }
    public long StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public long ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string EnrollDate { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Note { get; set; }
}

public class EnrollmentDetailDto : EnrollmentDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateEnrollmentRequest
{
    public long StudentId { get; set; }
    public long ClassId { get; set; }
    public string EnrollDate { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int Status { get; set; } = 1;
}

public class UpdateEnrollmentRequest
{
    public long StudentId { get; set; }
    public long ClassId { get; set; }
    public string EnrollDate { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int Status { get; set; }
}
