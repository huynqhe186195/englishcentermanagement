namespace EnglishCenter.Application.Features.Roles.Dtos;

public class GetRolesPagingRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
}
