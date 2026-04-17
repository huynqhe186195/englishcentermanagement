using System;
using System.Collections.Generic;

namespace EnglishCenter.Domain.Models;

public partial class AssignmentSubmission
{
    public long Id { get; set; }

    public long AssignmentId { get; set; }

    public long StudentId { get; set; }

    public string? SubmissionText { get; set; }

    public string? FileUrl { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public decimal? Score { get; set; }

    public string? Feedback { get; set; }

    public long? GradedByUserId { get; set; }

    public DateTime? GradedAt { get; set; }

    public virtual Assignment Assignment { get; set; } = null!;

    public virtual User? GradedByUser { get; set; }

    public virtual Student Student { get; set; } = null!;
}
