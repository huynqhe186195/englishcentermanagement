using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Classes;

public class TimetableModel : PageModel
{
    private readonly IApiClient _apiClient;

    public TimetableModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<TimetableItemDto> Items { get; set; } = new();

    [BindProperty(SupportsGet = true)] public string? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public string? ToDate { get; set; }

    public async Task OnGetAsync(long id)
    {
        var url = $"classes/{id}/timetable?PageNumber=1&PageSize=100";
        if (!string.IsNullOrWhiteSpace(FromDate)) url += $"&FromDate={FromDate}";
        if (!string.IsNullOrWhiteSpace(ToDate)) url += $"&ToDate={ToDate}";

        var data = await _apiClient.GetAsync<PagedResult<TimetableItemDto>>(url);
        if (data != null) Items = data.Items.ToList();
    }
}
