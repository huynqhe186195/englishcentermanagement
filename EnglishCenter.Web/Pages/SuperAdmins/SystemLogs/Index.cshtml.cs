using EnglishCenter.Web.Models;
using EnglishCenter.Web.Pages.Account;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.SuperAdmins.SystemLogs;

public class IndexModel : PageModel
{
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Module { get; set; }

    public List<AuditLogVm> Logs { get; set; } = new();

    public async Task OnGetAsync()
    {
        var url = "auditlogs?pageNumber=1&pageSize=30&SortBy=CreatedAt&SortDirection=desc";
        if (!string.IsNullOrWhiteSpace(Keyword))
        {
            url += $"&Action={Uri.EscapeDataString(Keyword.Trim())}";
        }
        if (!string.IsNullOrWhiteSpace(Module))
        {
            url += $"&EntityName={Uri.EscapeDataString(Module.Trim())}";
        }

        var paged = await _apiClient.GetAsync<Models.PagedResult<AuditLogVm>>(url);
        Logs = (List<AuditLogVm>)(paged?.Items ?? new List<AuditLogVm>());
    }

    public class AuditLogVm
    {
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
