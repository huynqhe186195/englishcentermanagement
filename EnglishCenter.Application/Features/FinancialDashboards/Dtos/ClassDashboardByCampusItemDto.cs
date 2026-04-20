using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.FinancialDashboards.Dtos;

public class ClassDashboardByCampusItemDto
{
    public long CampusId { get; set; }
    public string CampusCode { get; set; } = string.Empty;
    public string CampusName { get; set; } = string.Empty;

    public int ClassCount { get; set; }
    public int ActiveClassCount { get; set; }

    public int ActiveEnrollments { get; set; }
    public int SuspendedEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public int TransferredEnrollments { get; set; }
    public int CancelledEnrollments { get; set; }

    public int TotalSessions { get; set; }
    public int PlannedSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int CancelledSessions { get; set; }
    public int UpcomingSessions { get; set; }

    public decimal AttendanceRate { get; set; }
}
