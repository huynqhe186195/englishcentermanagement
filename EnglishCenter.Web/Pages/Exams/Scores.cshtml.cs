using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Exams;

public class ScoresModel : PageModel
{
    private readonly IApiClient _apiClient;
    private readonly IConfiguration _config;

    public ScoresModel(IApiClient apiClient, IConfiguration config)
    {
        _apiClient = apiClient;
        _config = config;
    }

    [BindProperty]
    public long SelectedExamId { get; set; }

    public List<SelectListItem> ExamOptions { get; set; } = new List<SelectListItem>();

    public string ApiBaseUrl { get; set; } = string.Empty;

    [BindProperty]
    public IFormFile? File { get; set; }

    public List<PassFailDto>? PassFailList { get; set; }

    public async Task OnGetAsync(long? selectedExamId)
    {
        var paged = await _apiClient.GetAsync<PagedResult<ExamDto>>("exams?pageNumber=1&pageSize=1000");
        var exams = paged?.Items ?? Enumerable.Empty<ExamDto>();
        ExamOptions = exams.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Title }).ToList();
        if (selectedExamId.HasValue)
        {
            SelectedExamId = selectedExamId.Value;
        }

        ApiBaseUrl = _config.GetValue<string>("Api:BaseUrl")?.TrimEnd('/') ?? string.Empty;
    }

    public async Task<IActionResult> OnGetDownloadTemplate(long examId)
    {
        var data = await _apiClient.GetFileAsync($"scores/template/{examId}");
        if (data == null) return RedirectToPage();
        return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"scores_template_exam_{examId}.xlsx");
    }

    public async Task<IActionResult> OnPostImportAsync()
    {
        if (SelectedExamId <= 0) return RedirectToPage();
        if (File == null || File.Length == 0)
        {
            ModelState.AddModelError("File", "Please choose a file.");
            return RedirectToPage(new { selectedExamId = SelectedExamId });
        }

        var result = await _apiClient.PostMultipartAsync<List<PassFailDto>>($"scores/import/{SelectedExamId}", File);
        PassFailList = result;

        if (result != null)
        {
            TempData["ToastMessage"] = "Import completed.";
            TempData["ToastType"] = "success";
        }
        else
        {
            TempData["ToastMessage"] = "Import failed.";
            TempData["ToastType"] = "error";
        }

        // reload exam list for select
        var paged = await _apiClient.GetAsync<PagedResult<ExamDto>>("exams?pageNumber=1&pageSize=1000");
        var exams = paged?.Items ?? Enumerable.Empty<ExamDto>();
        ExamOptions = exams.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Title }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnGetExport(long examId, bool passed)
    {
        var data = await _apiClient.GetFileAsync($"scores/export/{examId}?passed={passed}");
        if (data == null) return RedirectToPage();
        var fileName = $"scores_export_exam_{examId}_{(passed ? "passed" : "notpassed")}.xlsx";
        return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
