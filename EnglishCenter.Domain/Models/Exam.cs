using System;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Models;

public partial class Exam
{
    public long Id { get; set; }

    public long ClassId { get; set; }

    public string Title { get; set; } = null!;

    public int ExamType { get; set; }

    public DateTime ExamDate { get; set; }

    public decimal MaxScore { get; set; }

    public string? Description { get; set; }

    public long? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual User? CreatedByUser { get; set; }

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();
}
