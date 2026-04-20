using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.FinancialDashboards.Dtos;

public class RevenueByMonthItemDto
{
    public int Year { get; set; }
    public int Month { get; set; }

    public int InvoiceCount { get; set; }
    public int PaidInvoiceCount { get; set; }

    public decimal ExpectedRevenue { get; set; }
    public decimal CollectedRevenue { get; set; }
}
