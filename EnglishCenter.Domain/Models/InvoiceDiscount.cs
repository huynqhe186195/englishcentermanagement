using System;
using System.Collections.Generic;

namespace EnglishCenter.Infrastructure.Persistence.Models;

public partial class InvoiceDiscount
{
    public long Id { get; set; }

    public long InvoiceId { get; set; }

    public long DiscountId { get; set; }

    public decimal AppliedValue { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Discount Discount { get; set; } = null!;

    public virtual Invoice Invoice { get; set; } = null!;
}
