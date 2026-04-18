using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.Dashboards.Dtos;

public class StudentAtRiskDto
{
    public long StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public int SuspendedEnrollments { get; set; }
    public int TotalAttendanceRecords { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public decimal AttendanceRate { get; set; }

    public bool IsAtRiskByAttendance { get; set; }
    public bool IsAtRiskBySuspension { get; set; }
}
