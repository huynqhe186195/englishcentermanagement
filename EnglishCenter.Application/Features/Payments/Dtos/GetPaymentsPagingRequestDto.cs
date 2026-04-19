using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Common.Models;

namespace EnglishCenter.Application.Features.Payments.Dtos;

public class GetPaymentsPagingRequestDto : SortablePaginationRequest
{
    public long? InvoiceId { get; set; }
    public int? PaymentMethod { get; set; }
    public int? Status { get; set; }
    public string? TransactionCode { get; set; }
}
