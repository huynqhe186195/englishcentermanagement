using EnglishCenter.Application.Common.Models;

namespace EnglishCenter.Application.Features.Students.Dtos;

public class GetStudentsPagingRequestDto : SortablePaginationRequest
{
    public string? Keyword { get; set; }
    public int? Status { get; set; }
}