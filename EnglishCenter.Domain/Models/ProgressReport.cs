using System;
using System.Collections.Generic;

namespace EnglishCenter.Domain.Models;

public partial class ProgressReport
{
    public long Id { get; set; }

    public long StudentId { get; set; }

    public long ClassId { get; set; }

    public string ReportPeriod { get; set; } = null!;

    public decimal? ListeningScore { get; set; }

    public decimal? SpeakingScore { get; set; }

    public decimal? ReadingScore { get; set; }

    public decimal? WritingScore { get; set; }

    public decimal? VocabularyScore { get; set; }

    public decimal? GrammarScore { get; set; }

    public string? TeacherComment { get; set; }

    public string? Recommendation { get; set; }

    public long? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual User? CreatedByUser { get; set; }

    public virtual Student Student { get; set; } = null!;
}
