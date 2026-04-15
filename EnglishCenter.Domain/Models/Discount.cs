using System;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Models;

public partial class Discount
{
    public long Id { get; set; }

    public string DiscountCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int DiscountType { get; set; }

    public decimal Value { get; set; }

    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<InvoiceDiscount> InvoiceDiscounts { get; set; } = new List<InvoiceDiscount>();
}
