using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Enrollments;

public class IndexModel : PageModel
{
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IEnumerable<EnrollmentDto> Items { get; set; } = Enumerable.Empty<EnrollmentDto>();
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
    [BindProperty(SupportsGet = true)] public string? Keyword { get; set; }

    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }

    public async Task OnGetAsync()
    {
        var url = $"enrollments?PageNumber={PageNumber}&PageSize={PageSize}";
        if (!string.IsNullOrWhiteSpace(Keyword)) url += $"&Keyword={System.Net.WebUtility.UrlEncode(Keyword)}";

        var data = await _apiClient.GetAsync<PagedResult<EnrollmentDto>>(url);
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
        var ok = await _apiClient.DeleteAsync($"enrollments/{id}");
        if (!ok)
        {
            TempData["Error"] = "Delete failed.";
        }
        return RedirectToPage();
    }
}
