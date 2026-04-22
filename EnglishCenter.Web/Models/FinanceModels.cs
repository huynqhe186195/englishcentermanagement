namespace EnglishCenter.Web.Models;

public class InvoiceDto
{
    public long Id { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public long StudentId { get; set; }
    public long? ClassId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public string DueDate { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateInvoiceRequest
{
    public long StudentId { get; set; }
    public long CourseId { get; set; }
    public string DueDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string? Note { get; set; }
}

public class SelectClassForInvoiceRequest
{
    public long ClassId { get; set; }
}

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

public class CreatePaymentRequest
{
    public long InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public int PaymentMethod { get; set; } = 1;
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public string? TransactionCode { get; set; }
    public long? ReceivedByUserId { get; set; }
    public string? Note { get; set; }
}
