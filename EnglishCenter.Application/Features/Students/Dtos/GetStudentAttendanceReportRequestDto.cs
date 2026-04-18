using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.Students.Dtos;

public class GetStudentAttendanceReportRequestDto
{
    public long ClassId { get; set; }
    public bool SendWarningEmail { get; set; } = false;
}
