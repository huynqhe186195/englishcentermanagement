using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Request;

namespace EnglishCenter.Application.Features.Classes.Dtos;

public class GetClassesPagingRequestDto : PaginationRequest
{
    public string? Keyword { get; set; }
    public int? Status { get; set; }
}
