using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishCenter.Web.Pages.Enrollments;

public class EditModel : PageModel
{
    private readonly IApiClient _apiClient;

    public EditModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public UpdateEnrollmentRequest Input { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;
    public List<SelectListItem> Students { get; set; } = new();
    public List<SelectListItem> Classes { get; set; } = new();

    public async Task OnGetAsync(long id)
    {
        var data = await _apiClient.GetAsync<EnrollmentDetailDto>($"enrollments/{id}");
        if (data != null)
        {
            Input.StudentId = data.StudentId;
            Input.ClassId = data.ClassId;
            Input.EnrollDate = data.EnrollDate;
            Input.Note = data.Note;
            Input.Status = data.Status;
        }
        var students = await _apiClient.GetAsync<PagedResult<StudentSimpleDto>>("students?PageNumber=1&PageSize=1000");
        var classes = await _apiClient.GetAsync<PagedResult<ClassDto>>("classes?PageNumber=1&PageSize=1000");

        Students = students?.Items.Select(s => new SelectListItem(s.FullName, s.Id.ToString(), s.Id == Input.StudentId)).ToList() ?? new List<SelectListItem>();
        Classes = classes?.Items.Select(c => new SelectListItem(c.Name + " (" + c.ClassCode + ")", c.Id.ToString(), c.Id == Input.ClassId)).ToList() ?? new List<SelectListItem>();
    }

    public async Task<IActionResult> OnPostAsync(long id)
    {
        if (!ModelState.IsValid) return Page();
        var ok = await _apiClient.PutAsync($"enrollments/{id}", Input);
        if (!ok) { ErrorMessage = "Update failed."; return Page(); }
        return RedirectToPage("Index");
    }
}
