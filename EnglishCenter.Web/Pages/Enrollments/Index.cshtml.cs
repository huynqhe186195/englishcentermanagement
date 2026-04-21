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
    public int TotalCourses { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int TotalLearnedHours { get; set; }
    public int TotalPlannedHours { get; set; }

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

            TotalCourses = data.TotalRecords;
            ActiveCount = data.Items.Count(x => x.Status == 1);
            CompletedCount = data.Items.Count(x => x.Status == 3);
            TotalPlannedHours = data.Items.Count * 80;
            TotalLearnedHours = data.Items.Sum(x => GetProgressPercent(x.Id)) * 80 / 100;
        }
    }

    public int GetProgressPercent(long enrollmentId)
    {
        return (int)((enrollmentId * 17) % 55) + 35;
    }

    public string GetStatusText(int status)
    {
        return status switch
        {
            1 => "Đang học",
            2 => "Tạm dừng",
            3 => "Đã hoàn thành",
            4 => "Đã chuyển lớp",
            5 => "Đã hủy",
            _ => "Không xác định"
        };
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
