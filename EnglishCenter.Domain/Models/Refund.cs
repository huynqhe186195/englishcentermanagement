using System;
using System.Collections.Generic;

namespace EnglishCenter.Domain.Models;

public partial class Refund
{
    public long Id { get; set; }

    public long InvoiceId { get; set; }

    public decimal Amount { get; set; }

    public DateTime RefundDate { get; set; }

    public string? Reason { get; set; }

    public long? ProcessedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual User? ProcessedByUser { get; set; }
}
