using System;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Models;

public partial class ClassTeacher
{
    public long Id { get; set; }

    public long ClassId { get; set; }

    public long TeacherId { get; set; }

    public bool IsMainTeacher { get; set; }

    public DateTime AssignedFrom { get; set; }

    public DateTime? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Teacher Teacher { get; set; } = null!;
}
