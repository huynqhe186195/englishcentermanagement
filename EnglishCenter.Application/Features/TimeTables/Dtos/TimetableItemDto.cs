using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.Timetables.Dtos;

public class TimetableItemDto
{
    public long SessionId { get; set; }
    public long ClassId { get; set; }
    public int SessionNo { get; set; }
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public long? TeacherId { get; set; }
    public long? RoomId { get; set; }
    public string? Topic { get; set; }
    public string? Note { get; set; }
    public int Status { get; set; }
}
