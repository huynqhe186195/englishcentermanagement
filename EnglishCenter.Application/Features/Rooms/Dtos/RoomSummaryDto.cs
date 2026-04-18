using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.Rooms.Dtos;

public class RoomSummaryDto
{
    public long RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int Status { get; set; }

    public int TotalAssignedClasses { get; set; }
    public int TotalSessions { get; set; }
    public int PlannedSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int CancelledSessions { get; set; }
    public int UpcomingSessions { get; set; }
    public int TodaySessions { get; set; }
}
