using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishCenter.Web.Pages.Enrollments;

public class CreateModel : PageModel
{
    private readonly IApiClient _apiClient;

    public CreateModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public CreateEnrollmentRequest Input { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public List<SelectListItem> Students { get; set; } = new();
    public List<SelectListItem> Classes { get; set; } = new();

    public async Task OnGetAsync()
    {
        var students = await _apiClient.GetAsync<PagedResult<StudentSimpleDto>>("students?PageNumber=1&PageSize=1000");
        var classes = await _apiClient.GetAsync<PagedResult<ClassDto>>("classes?PageNumber=1&PageSize=1000");

        Students = students?.Items.Select(s => new SelectListItem(s.FullName, s.Id.ToString())).ToList() ?? new List<SelectListItem>();
        Classes = classes?.Items.Select(c => new SelectListItem(c.Name + " (" + c.ClassCode + ")", c.Id.ToString())).ToList() ?? new List<SelectListItem>();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var result = await _apiClient.PostAsync<CreateEnrollmentRequest, object>("enrollments", Input);
        if (result == null) { ErrorMessage = "Create failed."; return Page(); }
        return RedirectToPage("Index");
    }
}
