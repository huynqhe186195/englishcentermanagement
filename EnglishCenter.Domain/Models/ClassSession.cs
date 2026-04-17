using System;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Models;

public partial class ClassSession
{
    public long Id { get; set; }

    public long ClassId { get; set; }

    public int SessionNo { get; set; }

    public DateOnly SessionDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public long? RoomId { get; set; }

    public long? TeacherId { get; set; }

    public string? Topic { get; set; }

    public string? Note { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();

    public virtual Class Class { get; set; } = null!;

    public virtual Room? Room { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
