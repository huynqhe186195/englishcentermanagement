namespace EnglishCenter.Web.Models;

public class SuspendEnrollmentRequest
{
    using System.ComponentModel.DataAnnotations;

    [Required(ErrorMessage = "Reason is required.")]
    public string Reason { get; set; } = string.Empty;
}
