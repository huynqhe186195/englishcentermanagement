using System;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Models;

public partial class Notification
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int Channel { get; set; }

    public int TargetType { get; set; }

    public long? TargetId { get; set; }

    public int Status { get; set; }

    public DateTime? SentAt { get; set; }

    public long? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByUser { get; set; }
}
