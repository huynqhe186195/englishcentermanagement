using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Common.Models;

namespace EnglishCenter.Application.Features.Timetables.Dtos;

public class GetTimetableRequestDto : SortablePaginationRequest
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? Status { get; set; }
}
