namespace EnglishCenter.Application.Features.Users.Dtos;

public class UpdateUserRequestDto
{
    public string? PasswordHash { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Status { get; set; }
}
