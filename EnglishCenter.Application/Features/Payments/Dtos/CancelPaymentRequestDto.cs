using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.Payments.Dtos;

public class CancelPaymentRequestDto
{
    public string Reason { get; set; } = string.Empty;
}
