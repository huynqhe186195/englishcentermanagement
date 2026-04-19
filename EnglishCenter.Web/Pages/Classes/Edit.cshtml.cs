using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishCenter.Web.Pages.Classes;

public class EditModel : PageModel
{
    private readonly IApiClient _apiClient;

    public EditModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public UpdateClassRequest Input { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public List<SelectListItem> Courses { get; set; } = new();
    public List<SelectListItem> Campuses { get; set; } = new();
    public List<SelectListItem> Rooms { get; set; } = new();

    public async Task OnGetAsync(long id)
    {
        var data = await _apiClient.GetAsync<ClassDetailDto>($"classes/{id}");
        if (data != null)
        {
            Input.Id = data.Id;
            Input.CourseId = data.CourseId;
            Input.CampusId = data.CampusId;
            Input.RoomId = data.RoomId;
            Input.Name = data.Name;
            Input.StartDate = data.StartDate;
            Input.EndDate = data.EndDate;
            Input.MaxStudents = data.MaxStudents;
            Input.TuitionFee = data.TuitionFee;
            Input.Status = data.Status;
        }

        // load lookup lists
        var courses = await _apiClient.GetAsync<PagedResult<CourseSimpleDto>>("courses?PageNumber=1&PageSize=1000");
        var campuses = await _apiClient.GetAsync<PagedResult<CampusSimpleDto>>("campuses?PageNumber=1&PageSize=1000");
        var rooms = await _apiClient.GetAsync<PagedResult<RoomSimpleDto>>("rooms?PageNumber=1&PageSize=1000");

        Courses = courses?.Items.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == Input.CourseId)).ToList() ?? new List<SelectListItem>();
        Campuses = campuses?.Items.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == (Input.CampusId ?? 0))).ToList() ?? new List<SelectListItem>();
        Rooms = rooms?.Items.Select(r => new SelectListItem(r.Name, r.Id.ToString(), r.Id == (Input.RoomId ?? 0))).ToList() ?? new List<SelectListItem>();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var ok = await _apiClient.PutAsync($"classes/{Input.Id}", Input);
        if (!ok) { ErrorMessage = "Update failed."; return Page(); }
        return RedirectToPage("Index");
    }
}
