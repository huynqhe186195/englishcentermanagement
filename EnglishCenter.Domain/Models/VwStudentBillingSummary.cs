using System;
using System.Collections.Generic;

namespace EnglishCenter.Domain.Models;

public partial class VwStudentBillingSummary
{
    public long StudentId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string StudentName { get; set; } = null!;

    public int? TotalInvoices { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? TotalDiscount { get; set; }

    public decimal? TotalFinalAmount { get; set; }

    public decimal? TotalPaidAmount { get; set; }

    public decimal? TotalRefundedAmount { get; set; }

    public decimal? TotalOutstanding { get; set; }
}
