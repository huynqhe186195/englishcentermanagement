namespace EnglishCenter.Application.Features.Auth.Dtos;

public class RegisterStudentResponseDto
{
    public long UserId { get; set; }
    public long StudentId { get; set; }
    public string UserName { get; set; } = string.Empty;
}
