using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Request;

namespace EnglishCenter.Application.Features.Students.Dtos;

public class GetStudentsPagingRequestDto : PaginationRequest
{
    public string? Keyword { get; set; }
    public int? Status { get; set; }
}