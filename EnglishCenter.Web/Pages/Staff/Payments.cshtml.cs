using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishCenter.Web.Pages.Staff;

public class PaymentsModel : PageModel
{
    private readonly IApiClient _apiClient;

    public PaymentsModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<PaymentDto> Items { get; set; } = new();
    public List<SelectListItem> Invoices { get; set; } = new();

    [BindProperty(SupportsGet = true)] public long? InvoiceId { get; set; }
    [BindProperty] public CreatePaymentRequest CreateInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadAsync();
        if (InvoiceId.HasValue)
        {
            CreateInput.InvoiceId = InvoiceId.Value;
        }
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var ok = await _apiClient.PostAsync("payments", CreateInput);
        TempData["ToastMessage"] = ok ? "Tạo payment thành công." : "Tạo payment thất bại.";
        TempData["ToastType"] = ok ? "success" : "error";
        return RedirectToPage(new { invoiceId = CreateInput.InvoiceId });
    }

    private async Task LoadAsync()
    {
        var url = "payments?PageNumber=1&PageSize=30";
        if (InvoiceId.HasValue) url += $"&InvoiceId={InvoiceId.Value}";
        var data = await _apiClient.GetAsync<PagedResult<PaymentDto>>(url);
        Items = data?.Items ?? new List<PaymentDto>();

        var invoices = await _apiClient.GetAsync<PagedResult<InvoiceDto>>("invoices?PageNumber=1&PageSize=200");
        Invoices = invoices?.Items.Select(x => new SelectListItem($"{x.InvoiceNo} - {x.FinalAmount:N0}", x.Id.ToString())).ToList() ?? new List<SelectListItem>();
    }
}
