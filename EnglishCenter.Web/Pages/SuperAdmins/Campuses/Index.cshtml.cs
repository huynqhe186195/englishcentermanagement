using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.SuperAdmins.Campuses;

public class IndexModel : PageModel
{
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<CampusRowVm> Campuses { get; set; } = new();

    public async Task OnGetAsync()
    {
        var campusPaged = await _apiClient.GetAsync<PagedResult<CampusSimpleDto>>("campuses?pageNumber=1&pageSize=200");
        var campusRevenue = await _apiClient.GetAsync<List<RevenueByCampusItemDto>>("financialDashboard/revenue-by-campus")
            ?? new List<RevenueByCampusItemDto>();

        var campusItems = (campusPaged?.Items ?? new List<CampusSimpleDto>()).ToList();
        if (!campusItems.Any() && campusRevenue.Any())
        {
            // Fallback: if campuses endpoint returns empty for current session/scope,
            // still render rows from revenue endpoint to avoid blank page.
            campusItems = campusRevenue
                .Select(x => new CampusSimpleDto
                {
                    Id = x.CampusId,
                    CampusCode = x.CampusCode,
                    Name = x.CampusName
                })
                .ToList();
        }

        foreach (var c in campusItems)
        {
            var revenue = campusRevenue.FirstOrDefault(x => x.CampusId == c.Id);
            Campuses.Add(new CampusRowVm
            {
                CampusName = c.Name,
                CampusCode = c.CampusCode,
                Revenue = revenue?.CollectedRevenue ?? 0,
                InvoiceCount = revenue?.InvoiceCount ?? 0
            });
        }
    }

    public class CampusRowVm
    {
        public string CampusName { get; set; } = string.Empty;
        public string CampusCode { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int InvoiceCount { get; set; }
    }
}
