namespace EnglishCenter.Web.Models;

public class CourseSimpleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CampusSimpleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
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
