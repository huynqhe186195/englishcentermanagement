namespace EnglishCenter.Application.Features.Overrides.Dtos;

public class ExecuteOverrideRequestDto
{
    // Supported:
    // - INVOICE_CANCEL
    // - ENROLLMENT_SUSPEND
    // - CLASSSESSION_CANCEL
    public string ActionCode { get; set; } = string.Empty;
    public long TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Note { get; set; }
}
