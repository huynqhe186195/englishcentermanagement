using System;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Models;

public partial class Score
{
    public long Id { get; set; }

    public long ExamId { get; set; }

    public long StudentId { get; set; }

    public decimal ScoreValue { get; set; }

    public string? Remark { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Exam Exam { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
