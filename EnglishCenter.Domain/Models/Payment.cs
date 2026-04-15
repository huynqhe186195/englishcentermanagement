using System;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Models;

public partial class Payment
{
    public long Id { get; set; }

    public long InvoiceId { get; set; }

    public decimal Amount { get; set; }

    public int PaymentMethod { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? TransactionCode { get; set; }

    public long? ReceivedByUserId { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual User? ReceivedByUser { get; set; }
}
