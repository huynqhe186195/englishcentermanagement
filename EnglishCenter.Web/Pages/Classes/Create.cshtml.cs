using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

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
    public List<SelectListItem> Rooms { get; set; } = new();
    public string CurrentCampusName { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        SetCampusFromSession();

        // load lookup lists
        var courses = await _apiClient.GetAsync<PagedResult<CourseSimpleDto>>("courses?PageNumber=1&PageSize=1000");
        var rooms = await _apiClient.GetAsync<PagedResult<RoomSimpleDto>>("rooms?PageNumber=1&PageSize=1000");

        Courses = courses?.Items.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList() ?? new List<SelectListItem>();
        Rooms = rooms?.Items.Select(r => new SelectListItem(r.Name, r.Id.ToString())).ToList() ?? new List<SelectListItem>();

        await LoadCurrentCampusNameAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        SetCampusFromSession();

        if (!Input.CampusId.HasValue || Input.CampusId.Value <= 0)
        {
            ErrorMessage = "Không xác định được campus từ phiên đăng nhập. Vui lòng đăng nhập lại.";
            await OnGetAsync();
            return Page();
        }

        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var resp = await _apiClient.PostAsync<CreateClassRequest, object>("classes", Input);
        if (resp == null)
        {
            ErrorMessage = "Create failed.";
            await OnGetAsync();
            return Page();
        }

        return RedirectToPage("Index");
    }

    private void SetCampusFromSession()
    {
        var campusIdRaw = HttpContext.Session.GetString("CampusId");
        if (long.TryParse(campusIdRaw, out var campusId) && campusId > 0)
        {
            Input.CampusId = campusId;
        }
    }

    private async Task LoadCurrentCampusNameAsync()
    {
        if (!Input.CampusId.HasValue || Input.CampusId.Value <= 0)
        {
            CurrentCampusName = "Không xác định";
            return;
        }

        var campus = await _apiClient.GetAsync<CampusSimpleDto>($"campuses/{Input.CampusId.Value}");
        CurrentCampusName = campus?.Name ?? $"Campus #{Input.CampusId.Value}";
    }
}
