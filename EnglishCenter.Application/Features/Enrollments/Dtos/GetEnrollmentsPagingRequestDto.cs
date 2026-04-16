namespace EnglishCenter.Application.Features.Enrollments.Dtos;

public class GetEnrollmentsPagingRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public int? Status { get; set; }
}
