namespace EnglishCenter.Application.Features.Users.Dtos;

public class CreateUserRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Status { get; set; } = 1;
}
