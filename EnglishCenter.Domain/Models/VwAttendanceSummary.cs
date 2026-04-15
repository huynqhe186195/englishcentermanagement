using System;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Models;

public partial class VwAttendanceSummary
{
    public long StudentId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string StudentName { get; set; } = null!;

    public int? TotalAttendanceMarked { get; set; }

    public int? PresentCount { get; set; }

    public int? LateCount { get; set; }

    public int? AbsentExcusedCount { get; set; }

    public int? AbsentUnexcusedCount { get; set; }
}
