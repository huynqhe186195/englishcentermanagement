using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Exams;

public class EditModel : PageModel
{
    private readonly IApiClient _apiClient;

    public EditModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty(SupportsGet = true)]
    public long Id { get; set; }

    [BindProperty]
    public long ClassId { get; set; }

    [BindProperty]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    public int ExamType { get; set; } = 1;

    [BindProperty]
    public DateTime ExamDate { get; set; }

    [BindProperty]
    public decimal MaxScore { get; set; } = 10;

    [BindProperty]
    public string? Description { get; set; }

    public async Task OnGetAsync(long id)
    {
        Id = id;
        var dto = await _apiClient.GetAsync<EnglishCenter.Web.Models.ExamDetailDto>($"exams/{id}");
        if (dto != null)
        {
            ClassId = dto.ClassId;
            Title = dto.Title;
            ExamType = dto.ExamType;
            ExamDate = dto.ExamDate.ToLocalTime();
            MaxScore = dto.MaxScore;
            Description = dto.Description;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var update = new UpdateExamRequestDto
        {
            Title = Title,
            ExamType = ExamType,
            ExamDate = ExamDate.ToUniversalTime(),
            MaxScore = MaxScore,
            Description = Description
        };

        await _apiClient.PutAsync($"exams/{Id}", update);
        TempData["ToastMessage"] = "Exam updated.";
        TempData["ToastType"] = "success";
        return RedirectToPage("/Classes/Details", new { id = ClassId });
    }
}
