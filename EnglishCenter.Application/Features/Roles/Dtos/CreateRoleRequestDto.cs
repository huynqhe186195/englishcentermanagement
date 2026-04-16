namespace EnglishCenter.Application.Features.Roles.Dtos;

public class CreateRoleRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
