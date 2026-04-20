using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.Payments.Dtos;

public class PaymentDto
{
    public long Id { get; set; }
    public long InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public int PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? TransactionCode { get; set; }
    public long? ReceivedByUserId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
