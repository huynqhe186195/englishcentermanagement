using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Enrollments;

public class SuspendModel : PageModel
{
    private readonly IApiClient _apiClient;

    public SuspendModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public SuspendEnrollmentRequest Input { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public long ClassId { get; set; }

    public async Task OnGetAsync(long id)
    {
        // id is enrollment id
        var enrollment = await _apiClient.GetAsync<EnglishCenter.Web.Models.EnrollmentDetailDto>($"enrollments/{id}");
        if (enrollment != null) ClassId = enrollment.ClassId;
    }

    public async Task<IActionResult> OnPostAsync(long id)
    {
        var ok = await _apiClient.PutAsync($"enrollments/{id}/suspend", Input);
        if (!ok) { ErrorMessage = "Suspend failed."; return Page(); }
        return RedirectToPage("/Classes/Roster", new { id = ClassId });
    }
}
