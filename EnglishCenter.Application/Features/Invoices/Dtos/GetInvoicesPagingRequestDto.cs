using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Common.Models;

namespace EnglishCenter.Application.Features.Invoices.Dtos;

public class GetInvoicesPagingRequestDto : SortablePaginationRequest
{
    public string? InvoiceNo { get; set; }
    public long? StudentId { get; set; }
    public long? ClassId { get; set; }
    public int? Status { get; set; }
}
