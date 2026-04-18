using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.Enrollments.Dtos;

public class EnrollmentAttendancePolicyResultDto
{
    public long EnrollmentId { get; set; }
    public long StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public int ValidSessionCount { get; set; }
    public int AbsentCount { get; set; }
    public decimal AbsentRate { get; set; }

    public bool IsWarning { get; set; }
    public bool IsSuspended { get; set; }
    public string Message { get; set; } = string.Empty;
}
