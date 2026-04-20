using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.FinancialDashboards.Dtos;

public class TeacherWorkloadByCampusItemDto
{
    public long CampusId { get; set; }
    public string CampusCode { get; set; } = string.Empty;
    public string CampusName { get; set; } = string.Empty;

    public int TeacherCount { get; set; }
    public int ActiveTeacherCount { get; set; }

    public int TotalAssignedClasses { get; set; }
    public int TotalSessions { get; set; }
    public int PlannedSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int CancelledSessions { get; set; }
    public int UpcomingSessions { get; set; }
    public int TodaySessions { get; set; }
}
