using EnglishCenter.Application.Common.Models;

namespace EnglishCenter.Application.Features.ClassSessions.Dtos;

public class GetClassSessionsPagingRequestDto : SortablePaginationRequest
{
    public long? ClassId { get; set; }
    public int? Status { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}