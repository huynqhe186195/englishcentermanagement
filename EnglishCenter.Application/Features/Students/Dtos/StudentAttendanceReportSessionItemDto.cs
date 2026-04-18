using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace EnglishCenter.Application.Features.Students.Dtos;

public class StudentAttendanceReportSessionItemDto
{
    public long SessionId { get; set; }
    public int SessionNo { get; set; }
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public int SessionStatus { get; set; }
    public string SessionStatusText { get; set; } = string.Empty;

    public int? AttendanceStatus { get; set; }
    public string AttendanceStatusText { get; set; } = string.Empty;

    public string? Note { get; set; }
    public DateTime? CheckedAt { get; set; }
}
