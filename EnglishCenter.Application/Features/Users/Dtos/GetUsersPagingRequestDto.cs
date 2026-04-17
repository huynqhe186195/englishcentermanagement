namespace EnglishCenter.Application.Features.Users.Dtos;

public class GetUsersPagingRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public int? Status { get; set; }
}
