using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Exams;

public class ScheduleModel : PageModel
{
    private readonly IApiClient _apiClient;

    public ScheduleModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty(SupportsGet = true)]
    public long ClassId { get; set; }

    [BindProperty]
    public string Title { get; set; } = "";

    [BindProperty]
    public int ExamType { get; set; } = 1;

    [BindProperty]
    public DateTime SelectedDate { get; set; } = DateTime.UtcNow.Date;

    [BindProperty]
    public string SelectedTime { get; set; } = "09:00"; // HH:mm

    public List<AvailableSlotDto>? Slots { get; set; }

    [BindProperty]
    public string? SelectedStart { get; set; }

    public async Task OnGetAsync(long? classId)
    {
        if (classId.HasValue) ClassId = classId.Value;
    }

    public async Task<IActionResult> OnPostSearchAsync()
    {
        // compute slot start from SelectedDate + SelectedTime
        if (string.IsNullOrEmpty(SelectedTime)) SelectedTime = "09:00";
        if (!TimeSpan.TryParse(SelectedTime, out var ts)) ts = TimeSpan.FromHours(9);
        var slotStart = SelectedDate.Date + ts;
        var duration = ExamType == 1 ? 30 : 90;

        // ask API for available slot in the exact range
        var from = slotStart;
        var to = slotStart.AddMinutes(duration);
        var qs = $"exams/available-slots?classId={ClassId}&from={Uri.EscapeDataString(from.ToString("o"))}&to={Uri.EscapeDataString(to.ToString("o"))}&durationMinutes={duration}&stepMinutes={duration}";
        var slots = await _apiClient.GetAsync<List<AvailableSlotDto>>(qs);
        Slots = slots;
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrEmpty(SelectedStart)) return Page();
        var examDate = DateTime.Parse(SelectedStart, null, System.Globalization.DateTimeStyles.RoundtripKind);
        var dto = new CreateExamRequestDto
        {
            ClassId = ClassId,
            Title = Title,
            ExamType = ExamType,
            ExamDate = examDate,
            MaxScore = 10
        };

        var resp = await _apiClient.PostAsync<CreateExamRequestDto, object>("exams", dto);
        return RedirectToPage("/Classes/Details", new { id = ClassId });
    }
}
