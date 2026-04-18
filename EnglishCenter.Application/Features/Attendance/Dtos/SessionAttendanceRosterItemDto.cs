using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.Attendance.Dtos;

public class SessionAttendanceRosterItemDto
{
    public long StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public long EnrollmentId { get; set; }
    public int EnrollmentStatus { get; set; }

    public int? AttendanceStatus { get; set; }
    public string? Note { get; set; }
    public DateTime? CheckedAt { get; set; }
}
