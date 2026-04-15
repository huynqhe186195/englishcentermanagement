using System;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Models;

public partial class Enrollment
{
    public long Id { get; set; }

    public long StudentId { get; set; }

    public long ClassId { get; set; }

    public DateOnly EnrollDate { get; set; }

    public int Status { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
