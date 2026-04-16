using EnglishCenter.Application.Common.Models;

namespace EnglishCenter.Application.Features.Courses.Dtos;

public class GetCoursesPagingRequestDto : SortablePaginationRequest
{
    public string? Keyword { get; set; }
    public int? Status { get; set; }
}