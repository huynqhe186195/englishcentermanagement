using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Classes;

public class IndexModel : PageModel
{
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IEnumerable<ClassDto> Items { get; set; } = Enumerable.Empty<ClassDto>();
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
    [BindProperty(SupportsGet = true)] public string? Keyword { get; set; }
    [BindProperty(SupportsGet = true)] public int? Status { get; set; }

    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }

    public async Task OnGetAsync()
    {
        var url = $"classes?PageNumber={PageNumber}&PageSize={PageSize}";
        if (!string.IsNullOrWhiteSpace(Keyword)) url += $"&Keyword={System.Net.WebUtility.UrlEncode(Keyword)}";
        if (Status.HasValue) url += $"&Status={Status.Value}";

        var data = await _apiClient.GetAsync<PagedResult<ClassDto>>(url);
        if (data != null)
        {
            Items = data.Items;
            PageNumber = data.PageNumber;
            PageSize = data.PageSize;
            TotalPages = data.TotalPages;
            TotalRecords = data.TotalRecords;
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(long id)
    {
        var ok = await _apiClient.DeleteAsync($"classes/{id}");
        if (!ok)
        {
            TempData["Error"] = "Delete failed.";
        }
        return RedirectToPage();
    }
}
