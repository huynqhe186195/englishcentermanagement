using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishCenter.Web.Pages.Staff;

public class InvoicesModel : PageModel
{
    private readonly IApiClient _apiClient;

    public InvoicesModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<InvoiceDto> Items { get; set; } = new();
    public List<SelectListItem> Students { get; set; } = new();
    public List<SelectListItem> Courses { get; set; } = new();

    [BindProperty(SupportsGet = true)] public string? InvoiceNo { get; set; }
    [BindProperty(SupportsGet = true)] public int? Status { get; set; }

    [BindProperty] public CreateInvoiceRequest CreateInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var ok = await _apiClient.PostAsync("invoices", CreateInput);
        TempData["ToastMessage"] = ok ? "Tạo invoice thành công." : "Tạo invoice thất bại.";
        TempData["ToastType"] = ok ? "success" : "error";
        return RedirectToPage();
    }

    private async Task LoadDataAsync()
    {
        var url = "invoices?PageNumber=1&PageSize=20";
        if (!string.IsNullOrWhiteSpace(InvoiceNo)) url += $"&InvoiceNo={System.Net.WebUtility.UrlEncode(InvoiceNo)}";
        if (Status.HasValue) url += $"&Status={Status.Value}";

        var invoices = await _apiClient.GetAsync<PagedResult<InvoiceDto>>(url);
        Items = (List<InvoiceDto>)(invoices?.Items ?? new List<InvoiceDto>());

        var students = await _apiClient.GetAsync<PagedResult<StudentSimpleDto>>("students?PageNumber=1&PageSize=1000");
        Students = students?.Items.Select(x => new SelectListItem(x.FullName, x.Id.ToString())).ToList() ?? new List<SelectListItem>();

        var courses = await _apiClient.GetAsync<PagedResult<CourseDto>>("courses?PageNumber=1&PageSize=1000");
        Courses = courses?.Items.Select(x => new SelectListItem($"{x.CourseCode} - {x.Name}", x.Id.ToString())).ToList() ?? new List<SelectListItem>();
    }
}
