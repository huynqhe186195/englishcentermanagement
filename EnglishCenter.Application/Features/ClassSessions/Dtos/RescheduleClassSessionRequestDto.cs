using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace EnglishCenter.Application.Features.ClassSessions.Dtos;

public class RescheduleClassSessionRequestDto
{
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public long? RoomId { get; set; }
    public long? TeacherId { get; set; }
    public string? Note { get; set; }
}
