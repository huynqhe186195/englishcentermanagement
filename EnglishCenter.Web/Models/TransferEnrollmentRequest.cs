namespace EnglishCenter.Web.Models;

using System.ComponentModel.DataAnnotations;

public class TransferEnrollmentRequest
{
    [Required(ErrorMessage = "Please select a target class.")]
    public long TargetClassId { get; set; }
    public string? Note { get; set; }
}
