using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.Staff;

public class DashboardModel : PageModel
{
    private readonly IApiClient _apiClient;

    public DashboardModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public int PendingClassSelection { get; set; }
    public int InvoicesToday { get; set; }
    public int PaymentsToday { get; set; }
    public List<InvoiceDto> RecentInvoices { get; set; } = new();

    public async Task OnGetAsync()
    {
        var invoices = await _apiClient.GetAsync<PagedResult<InvoiceDto>>("invoices?PageNumber=1&PageSize=50");
        var payments = await _apiClient.GetAsync<PagedResult<PaymentDto>>("payments?PageNumber=1&PageSize=50");

        if (invoices != null)
        {
            var today = DateTime.Today;
            PendingClassSelection = invoices.Items.Count(x => x.Status == 3 && !x.ClassId.HasValue);
            InvoicesToday = invoices.Items.Count(x => x.CreatedAt.Date == today);
            RecentInvoices = invoices.Items.OrderByDescending(x => x.CreatedAt).Take(8).ToList();
        }

        if (payments != null)
        {
            var today = DateTime.Today;
            PaymentsToday = payments.Items.Count(x => x.PaymentDate.Date == today || x.CreatedAt.Date == today);
        }
    }
}
