using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishCenter.Web.Pages.Classes;

public class CreateModel : PageModel
{
    private readonly IApiClient _apiClient;

    public CreateModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public CreateClassRequest Input { get; set; } = new();

    public List<SelectListItem> Courses { get; set; } = new();
    public List<SelectListItem> Campuses { get; set; } = new();
    public List<SelectListItem> Rooms { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public async Task OnGetAsync()
    {
        // load lookup lists
        var courses = await _apiClient.GetAsync<PagedResult<CourseSimpleDto>>("courses?PageNumber=1&PageSize=1000");
        var campuses = await _apiClient.GetAsync<PagedResult<CampusSimpleDto>>("campuses?PageNumber=1&PageSize=1000");
        var rooms = await _apiClient.GetAsync<PagedResult<RoomSimpleDto>>("rooms?PageNumber=1&PageSize=1000");

        Courses = courses?.Items.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList() ?? new List<SelectListItem>();
        Campuses = campuses?.Items.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList() ?? new List<SelectListItem>();
        Rooms = rooms?.Items.Select(r => new SelectListItem(r.Name, r.Id.ToString())).ToList() ?? new List<SelectListItem>();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // map date string to DateOnly expected format yyyy-MM-dd
        var resp = await _apiClient.PostAsync<CreateClassRequest, object>("classes", Input);
        if (resp == null) { ErrorMessage = "Create failed."; return Page(); }

        return RedirectToPage("Index");
    }
}
