using System;
using System.Collections.Generic;

namespace EnglishCenter.Domain.Models;

public partial class Assignment
{
    public long Id { get; set; }

    public long ClassId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public decimal? MaxScore { get; set; }

    public long? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();

    public virtual Class Class { get; set; } = null!;

    public virtual User? CreatedByUser { get; set; }
}
