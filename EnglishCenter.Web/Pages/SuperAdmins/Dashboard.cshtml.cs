using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.SuperAdmins;

public class DashboardModel : PageModel
{
    private readonly IApiClient _apiClient;

    public DashboardModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public int TotalUsers { get; set; }
    public int TotalCampuses { get; set; }
    public int TotalStudents { get; set; }
    public decimal TotalCollectedRevenue { get; set; }
    public List<RevenueByCampusItemDto> TopCampuses { get; set; } = new();
    public List<AuditLogVm> RecentActivities { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalUsers = (await _apiClient.GetAsync<PagedResult<UserDto>>("users?pageNumber=1&pageSize=1"))?.TotalRecords ?? 0;
        TotalCampuses = (await _apiClient.GetAsync<PagedResult<CampusSimpleDto>>("campuses?pageNumber=1&pageSize=1"))?.TotalRecords ?? 0;
        TotalStudents = (await _apiClient.GetAsync<PagedResult<object>>("students?pageNumber=1&pageSize=1"))?.TotalRecords ?? 0;

        var revenueSummary = await _apiClient.GetAsync<RevenueSummaryDto>("financialDashboard/revenue-summary");
        TotalCollectedRevenue = revenueSummary?.TotalCollectedRevenue ?? 0;

        TopCampuses = (await _apiClient.GetAsync<List<RevenueByCampusItemDto>>("financialDashboard/revenue-by-campus") ?? new List<RevenueByCampusItemDto>())
            .OrderByDescending(x => x.CollectedRevenue)
            .Take(3)
            .ToList();

        var logs = await _apiClient.GetAsync<PagedResult<AuditLogVm>>("auditlogs?pageNumber=1&pageSize=3&SortBy=CreatedAt&SortDirection=desc");
        RecentActivities = (List<AuditLogVm>)(logs?.Items ?? new List<AuditLogVm>());
    }

    public class AuditLogVm
    {
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
