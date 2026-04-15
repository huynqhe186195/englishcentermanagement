using System;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Models;

public partial class StudentParent
{
    public long Id { get; set; }

    public long StudentId { get; set; }

    public long ParentId { get; set; }

    public string Relationship { get; set; } = null!;

    public bool IsPrimaryContact { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Parent Parent { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
