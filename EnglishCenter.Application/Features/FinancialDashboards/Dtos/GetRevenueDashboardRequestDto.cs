using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.FinancialDashboards.Dtos;

public class GetRevenueDashboardRequestDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
