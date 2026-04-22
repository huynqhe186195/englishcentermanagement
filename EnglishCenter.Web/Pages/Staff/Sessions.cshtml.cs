using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishCenter.Web.Pages.Staff;

public class SessionsModel : PageModel
{
    private readonly IApiClient _apiClient;

    public SessionsModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<ClassSessionDto> Items { get; set; } = new();
    public List<SelectListItem> Classes { get; set; } = new();
    [BindProperty] public CreateClassSessionRequest CreateInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var ok = await _apiClient.PostAsync("classsessions", CreateInput);
        TempData["ToastMessage"] = ok ? "Tạo session thành công." : "Tạo session thất bại.";
        TempData["ToastType"] = ok ? "success" : "error";
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        var sessions = await _apiClient.GetAsync<PagedResult<ClassSessionDto>>("classsessions?PageNumber=1&PageSize=30");
        Items = sessions?.Items ?? new List<ClassSessionDto>();

        var classes = await _apiClient.GetAsync<PagedResult<ClassDto>>("classes?PageNumber=1&PageSize=1000");
        Classes = classes?.Items.Select(x => new SelectListItem($"{x.ClassCode} - {x.Name}", x.Id.ToString())).ToList() ?? new List<SelectListItem>();
    }
}
