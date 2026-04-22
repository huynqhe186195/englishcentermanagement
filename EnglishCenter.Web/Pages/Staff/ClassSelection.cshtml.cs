using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishCenter.Web.Pages.Staff;

public class ClassSelectionModel : PageModel
{
    private readonly IApiClient _apiClient;

    public ClassSelectionModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<InvoiceDto> PaidWithoutClass { get; set; } = new();
    public List<SelectListItem> Classes { get; set; } = new();

    [BindProperty(SupportsGet = true)] public long? InvoiceId { get; set; }
    [BindProperty] public long SelectedInvoiceId { get; set; }
    [BindProperty] public SelectClassForInvoiceRequest Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        var invoices = await _apiClient.GetAsync<PagedResult<InvoiceDto>>("invoices?PageNumber=1&PageSize=100&Status=3");
        PaidWithoutClass = invoices?.Items.Where(x => !x.ClassId.HasValue).ToList() ?? new List<InvoiceDto>();

        if (InvoiceId.HasValue)
        {
            SelectedInvoiceId = InvoiceId.Value;
        }
        else if (PaidWithoutClass.Any())
        {
            SelectedInvoiceId = PaidWithoutClass.First().Id;
        }

        var classes = await _apiClient.GetAsync<PagedResult<ClassDto>>("classes?PageNumber=1&PageSize=1000&Status=1");
        Classes = classes?.Items.Select(x => new SelectListItem($"{x.ClassCode} - {x.Name}", x.Id.ToString())).ToList() ?? new List<SelectListItem>();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var ok = await _apiClient.PostAsync($"invoices/{SelectedInvoiceId}/select-class", Input);
        TempData["ToastMessage"] = ok ? "Chọn lớp thành công." : "Chọn lớp thất bại.";
        TempData["ToastType"] = ok ? "success" : "error";
        return RedirectToPage();
    }
}
