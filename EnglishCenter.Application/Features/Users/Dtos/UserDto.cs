namespace EnglishCenter.Application.Features.Users.Dtos;

public class UserDto
{
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Status { get; set; }

    public List<string> RoleNames { get; set; } = new();
    public string RoleDisplay { get; set; } = string.Empty;
}
