using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.FinancialDashboards.Dtos;

public class RevenueSummaryDto
{
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int UnpaidInvoices { get; set; }
    public int CancelledInvoices { get; set; }

    public decimal TotalExpectedRevenue { get; set; }
    public decimal TotalCollectedRevenue { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalRefundedAmount { get; set; }
}
