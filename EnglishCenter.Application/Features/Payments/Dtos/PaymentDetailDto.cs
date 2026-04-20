using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace EnglishCenter.Application.Features.Payments.Dtos;

public class PaymentDetailDto
{
    public long Id { get; set; }
    public long InvoiceId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;

    public long StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentFullName { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public int PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? TransactionCode { get; set; }
    public long? ReceivedByUserId { get; set; }
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }
}
