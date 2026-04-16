using EnglishCenter.Application.Commons.Models.Request;

namespace EnglishCenter.Application.Common.Models;

public class SortablePaginationRequest : PaginationRequest
{
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
}