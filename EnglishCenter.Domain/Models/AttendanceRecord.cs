using System;
using System.Collections.Generic;

namespace EnglishCenter.Domain.Models;

public partial class AttendanceRecord
{
    public long Id { get; set; }

    public long SessionId { get; set; }

    public long StudentId { get; set; }

    public int Status { get; set; }

    public string? Note { get; set; }

    public DateTime CheckedAt { get; set; }

    public long? CheckedByUserId { get; set; }

    public virtual User? CheckedByUser { get; set; }

    public virtual ClassSession Session { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
