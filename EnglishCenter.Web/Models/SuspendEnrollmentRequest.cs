using System.ComponentModel.DataAnnotations;

namespace EnglishCenter.Web.Models;

using System.ComponentModel.DataAnnotations;

public class SuspendEnrollmentRequest
{
    [Required(ErrorMessage = "Reason is required.")]
    public string Reason { get; set; } = string.Empty;
}