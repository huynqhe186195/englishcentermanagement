using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.SuperAdmins.Financial;

public class IndexModel : PageModel
{
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public RevenueSummaryDto Summary { get; set; } = new();
    public List<RevenueByCampusItemDto> RevenueByCampus { get; set; } = new();

    public async Task OnGetAsync()
    {
        Summary = await _apiClient.GetAsync<RevenueSummaryDto>("financialDashboard/revenue-summary") ?? new RevenueSummaryDto();
        RevenueByCampus = await _apiClient.GetAsync<List<RevenueByCampusItemDto>>("financialDashboard/revenue-by-campus")
            ?? new List<RevenueByCampusItemDto>();
    }
}
