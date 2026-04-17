using System;
using System.Collections.Generic;

namespace EnglishCenter.Domain.Models;

public partial class ClassSchedule
{
    public long Id { get; set; }

    public long ClassId { get; set; }

    public int DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public long? RoomId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Room? Room { get; set; }
}
