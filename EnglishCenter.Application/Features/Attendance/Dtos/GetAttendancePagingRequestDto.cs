using EnglishCenter.Application.Common.Models;

namespace EnglishCenter.Application.Features.Attendance.Dtos;

public class GetAttendancePagingRequestDto : SortablePaginationRequest
{
    public long? SessionId { get; set; }
    public long? StudentId { get; set; }
    public int? Status { get; set; }
}