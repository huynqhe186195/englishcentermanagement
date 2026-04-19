using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishCenter.Web.Pages.Enrollments;

public class TransferModel : PageModel
{
    private readonly IApiClient _apiClient;

    public TransferModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public TransferEnrollmentRequest Input { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public long ClassId { get; set; }
    public List<SelectListItem> Classes { get; set; } = new();

    public async Task OnGetAsync(long id)
    {
        var enrollment = await _apiClient.GetAsync<EnglishCenter.Web.Models.EnrollmentDetailDto>($"enrollments/{id}");
        if (enrollment != null) ClassId = enrollment.ClassId;
        // load classes for selection (exclude current class)
        var classesData = await _apiClient.GetAsync<PagedResult<ClassDto>>("classes?PageNumber=1&PageSize=1000");
        if (classesData != null)
        {
            Classes = classesData.Items
                .Where(c => c.Id != ClassId)
                .Select(c => new SelectListItem($"{c.Name} ({c.ClassCode})", c.Id.ToString()))
                .ToList();
        }
    }

    public async Task<IActionResult> OnPostAsync(long id)
    {
        var newId = await _apiClient.PostAsync<TransferEnrollmentRequest, object>($"enrollments/{id}/transfer", Input);
        if (newId == null)
        {
            ErrorMessage = "Transfer failed.";
            // reload classes for form redisplay
            var classesData = await _apiClient.GetAsync<PagedResult<ClassDto>>("classes?PageNumber=1&PageSize=1000");
            if (classesData != null)
            {
                Classes = classesData.Items
                    .Where(c => c.Id != ClassId)
                    .Select(c => new SelectListItem($"{c.Name} ({c.ClassCode})", c.Id.ToString())).ToList();
            }
            return Page();
        }

        TempData["Success"] = "Enrollment transferred successfully.";
        return RedirectToPage("/Classes/Roster", new { id = ClassId });
    }
}
