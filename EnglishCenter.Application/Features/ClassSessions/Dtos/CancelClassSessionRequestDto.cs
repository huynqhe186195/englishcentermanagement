using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.ClassSessions.Dtos;

public class CancelClassSessionRequestDto
{
    public string Reason { get; set; } = string.Empty;
}
