using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.Teacher;

public class AttendanceModel : PageModel
{
    private readonly IApiClient _apiClient;

    public AttendanceModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty(SupportsGet = true)]
    public long? SessionId { get; set; }

    [BindProperty]
    public List<AttendanceInput> Items { get; set; } = new();

    [BindProperty]
    public string? CompleteNote { get; set; }

    public long? TeacherId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<TimetableItemDto> Sessions { get; set; } = new();
    public TimetableItemDto? SelectedSession { get; set; }
    public List<SessionAttendanceRosterItemDto> Roster { get; set; } = new();
    public int ClassRosterCount { get; set; }
    public int ActiveEnrollmentCount { get; set; }

    public bool CanEditAttendance { get; set; }
    public bool CanCompleteSession { get; set; }
    public bool CanCancelCompleteSession { get; set; }

    public string? Message { get; set; }

    private const int PlannedStatus = 1;
    private const int CompletedStatus = 2;

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        await LoadDataAsync();

        if (!SessionId.HasValue || !CanEditAttendance)
        {
            Message = "Chỉ có thể sửa điểm danh trong ngày diễn ra session và khi session chưa hoàn tất.";
            return Page();
        }

        var request = new MarkAttendanceRequest
        {
            SessionId = SessionId.Value,
            Items = Items.Select(x => new MarkAttendanceItemRequest
            {
                StudentId = x.StudentId,
                Status = x.Status,
                Note = x.Note
            }).ToList()
        };

        var ok = await _apiClient.PostAsync("attendance/mark", request);
        Message = ok ? "Đã lưu điểm danh thành công." : "Lưu điểm danh thất bại.";

        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCompleteAsync()
    {
        await LoadDataAsync();

        if (!SessionId.HasValue || !CanCompleteSession)
        {
            Message = "Chỉ có thể hoàn tất buổi học trong ngày diễn ra session khi session chưa hoàn tất.";
            return Page();
        }

        var ok = await _apiClient.PutAsync($"classsessions/{SessionId.Value}/complete", new { Note = CompleteNote });
        Message = ok ? "Đã hoàn tất buổi học." : "Không thể hoàn tất buổi học.";

        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCancelCompleteAsync()
    {
        await LoadDataAsync();

        if (!SessionId.HasValue || !CanCancelCompleteSession)
        {
            Message = "Chỉ có thể mở lại buổi học đã hoàn tất trong chính ngày diễn ra session.";
            return Page();
        }

        var ok = await _apiClient.PutAsync($"classsessions/{SessionId.Value}/reopen", new { Note = CompleteNote });
        Message = ok ? "Đã mở lại buổi học để tiếp tục chỉnh sửa điểm danh." : "Không thể mở lại buổi học.";

        await LoadDataAsync();
        return Page();
    }

    private async Task LoadDataAsync()
    {
        var me = await _apiClient.GetAsync<CurrentUserDto>("auth/me");
        TeacherId = me?.TeacherId;
        FullName = me?.FullName ?? string.Empty;
        UserName = me?.UserName ?? string.Empty;

        if (!TeacherId.HasValue)
        {
            return;
        }

        var from = DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd");
        var to = DateTime.Today.AddDays(14).ToString("yyyy-MM-dd");

        var timetable = await _apiClient.GetAsync<PagedResult<TimetableItemDto>>(
            $"teachers/{TeacherId.Value}/timetable?PageNumber=1&PageSize=300&FromDate={from}&ToDate={to}&SortBy=SessionDate&SortDirection=asc");

        Sessions = timetable?.Items?
            .OrderBy(x => x.SessionDate)
            .ThenBy(x => x.StartTime)
            .ToList()
            ?? new List<TimetableItemDto>();

        if (!SessionId.HasValue && Sessions.Any())
        {
            SessionId = Sessions.First().SessionId;
        }

        if (!SessionId.HasValue)
        {
            return;
        }

        SelectedSession = Sessions.FirstOrDefault(x => x.SessionId == SessionId.Value);

        Roster = await _apiClient.GetAsync<List<SessionAttendanceRosterItemDto>>(
            $"attendance/session/{SessionId.Value}/roster") ?? new List<SessionAttendanceRosterItemDto>();

        if (SelectedSession != null)
        {
            var classRoster = await _apiClient.GetAsync<List<ClassRosterItemDto>>(
                $"classes/{SelectedSession.ClassId}/roster") ?? new List<ClassRosterItemDto>();

            ClassRosterCount = classRoster.Count;
            ActiveEnrollmentCount = classRoster.Count(x => x.EnrollmentStatus == 1);
        }
        else
        {
            ClassRosterCount = 0;
            ActiveEnrollmentCount = 0;
        }

        SetPermissions();

        if (!Items.Any() || Items.Count != Roster.Count)
        {
            Items = Roster.Select(x => new AttendanceInput
            {
                StudentId = x.StudentId,
                Status = x.AttendanceStatus ?? 0,
                Note = x.Note
            }).ToList();
        }
    }

    private void SetPermissions()
    {
        CanEditAttendance = false;
        CanCompleteSession = false;
        CanCancelCompleteSession = false;

        if (SelectedSession == null)
        {
            return;
        }

        if (!DateOnly.TryParse(SelectedSession.SessionDate, out var sessionDate))
        {
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var isToday = sessionDate == today;
        var isCompleted = SelectedSession.Status == CompletedStatus;

        CanEditAttendance = isToday && !isCompleted;
        CanCompleteSession = isToday && !isCompleted;
        CanCancelCompleteSession = isToday && isCompleted;
    }

    public class AttendanceInput
    {
        public long StudentId { get; set; }
        public int Status { get; set; }
        public string? Note { get; set; }
    }
}