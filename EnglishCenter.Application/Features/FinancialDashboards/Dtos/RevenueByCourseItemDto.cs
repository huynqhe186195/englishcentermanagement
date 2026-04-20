using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.FinancialDashboards.Dtos;

public class RevenueByCourseItemDto
{
    public long CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;

    public int InvoiceCount { get; set; }
    public int PaidInvoiceCount { get; set; }

    public decimal ExpectedRevenue { get; set; }
    public decimal CollectedRevenue { get; set; }
}
