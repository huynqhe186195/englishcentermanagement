using System;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Models;

public partial class Course
{
    public long Id { get; set; }

    public string CourseCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Level { get; set; }

    public int? AgeMin { get; set; }

    public int? AgeMax { get; set; }

    public int TotalSessions { get; set; }

    public decimal DefaultFee { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
