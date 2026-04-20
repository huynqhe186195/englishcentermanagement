using System.ComponentModel.DataAnnotations;

namespace EnglishCenter.Web.Models;

public class SuspendEnrollmentRequest
{
    [Required(ErrorMessage = "Reason is required.")]
    public string Reason { get; set; } = string.Empty;
}