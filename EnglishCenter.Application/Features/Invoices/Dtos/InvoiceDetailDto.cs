using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.Invoices.Dtos;

public class InvoiceDetailDto
{
    public long Id { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public long StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentFullName { get; set; } = string.Empty;

    public long? ClassId { get; set; }
    public string? ClassCode { get; set; }
    public string? ClassName { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RefundedAmount { get; set; }

    public DateOnly DueDate { get; set; }
    public int Status { get; set; }
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
