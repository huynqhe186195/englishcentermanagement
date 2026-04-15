using System;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Models;

public partial class Invoice
{
    public long Id { get; set; }

    public string InvoiceNo { get; set; } = null!;

    public long StudentId { get; set; }

    public long? ClassId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RefundedAmount { get; set; }

    public DateTime? DueDate { get; set; }

    public int Status { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Class? Class { get; set; }

    public virtual ICollection<InvoiceDiscount> InvoiceDiscounts { get; set; } = new List<InvoiceDiscount>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual Student Student { get; set; } = null!;
}
