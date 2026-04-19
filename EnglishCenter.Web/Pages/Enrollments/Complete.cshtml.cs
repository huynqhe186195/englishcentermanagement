using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Enrollments;

public class CompleteModel : PageModel
{
    private readonly IApiClient _apiClient;

    public CompleteModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public CompleteEnrollmentRequest Input { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public long ClassId { get; set; }

    public async Task OnGetAsync(long id)
    {
        var enrollment = await _apiClient.GetAsync<EnglishCenter.Web.Models.EnrollmentDetailDto>($"enrollments/{id}");
        if (enrollment != null) ClassId = enrollment.ClassId;
    }

    public async Task<IActionResult> OnPostAsync(long id)
    {
        var ok = await _apiClient.PutAsync($"enrollments/{id}/complete", Input);
        if (!ok) { ErrorMessage = "Complete failed."; return Page(); }
        return RedirectToPage("/Classes/Roster", new { id = ClassId });
    }
}
