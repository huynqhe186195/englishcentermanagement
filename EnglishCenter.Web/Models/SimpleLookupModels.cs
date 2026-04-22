namespace EnglishCenter.Web.Models;

public class CourseSimpleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CampusSimpleDto
{
    public long Id { get; set; }
    public string CampusCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CampusDetailDto
{
    public long Id { get; set; }
    public string CampusCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public int Status { get; set; }
}

public class RoomSimpleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class StudentSimpleDto
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
}
