using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.FinancialDashboards.Dtos;

public class RevenueByCampusItemDto
{
    public long CampusId { get; set; }
    public string CampusCode { get; set; } = string.Empty;
    public string CampusName { get; set; } = string.Empty;

    public int InvoiceCount { get; set; }
    public int PaidInvoiceCount { get; set; }
    public int UnpaidInvoiceCount { get; set; }
    public int CancelledInvoiceCount { get; set; }

    public decimal ExpectedRevenue { get; set; }
    public decimal CollectedRevenue { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal RefundedAmount { get; set; }
}
