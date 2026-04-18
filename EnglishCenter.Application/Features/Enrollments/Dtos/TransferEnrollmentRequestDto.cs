using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.Enrollments.Dtos;

public class TransferEnrollmentRequestDto
{
    public long TargetClassId { get; set; }
    public string? Note { get; set; }
}
