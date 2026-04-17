using System;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Models;

public partial class VwClassEnrollmentSummary
{
    public long ClassId { get; set; }

    public string ClassCode { get; set; } = null!;

    public string ClassName { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public int MaxStudents { get; set; }

    public int? CurrentStudents { get; set; }

    public int? RemainingSlots { get; set; }
}
