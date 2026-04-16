using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Request;

namespace EnglishCenter.Application.Features.Courses.Dtos;

public class GetCoursesPagingRequestDto : PaginationRequest
{
    public string? Keyword { get; set; }
    public int? Status { get; set; }
}