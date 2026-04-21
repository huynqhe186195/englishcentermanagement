using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;

namespace EnglishCenter.Web.Pages.Student;

public class ScheduleModel : PageModel
{
    private readonly IApiClient _apiClient;

    public ScheduleModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)] public string? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public string? Month { get; set; }

    public List<EnrollmentDto> Enrollments { get; set; } = new();
    public List<TimetableItemDto> Items { get; set; } = new();
    public List<DateOnly> WeekDays { get; set; } = new();
    public List<WeekOptionVm> WeekOptions { get; set; } = new();
    public List<MonthOptionVm> MonthOptions { get; set; } = new();

    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }
    public DateOnly PrevWeekStart => WeekStart.AddDays(-7);
    public DateOnly NextWeekStart => WeekStart.AddDays(7);

    public string DataSourceNote { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));

        WeekStart = DateOnly.TryParse(FromDate, out var from) ? from : monday;
        WeekEnd = WeekStart.AddDays(6);

        MonthOptions = Enumerable.Range(1, 12)
            .Select(m => new MonthOptionVm
            {
                Value = new DateOnly(today.Year, m, 1).ToString("yyyy-MM"),
                Label = $"Tháng {m:00}/{today.Year}"
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(Month) && DateOnly.TryParse($"{Month}-01", out var monthStart))
        {
            WeekStart = monthStart.AddDays(-(((int)monthStart.DayOfWeek + 6) % 7));
            WeekEnd = WeekStart.AddDays(6);
        }

        var me = await _apiClient.GetAsync<CurrentUserDto>("auth/me");
        if (me != null)
        {
            UserName = me.UserName;
            FullName = me.FullName;
        }

        var enrollmentData = await _apiClient.GetAsync<PagedResult<EnrollmentDto>>("enrollments?PageNumber=1&PageSize=100");
        var allEnrollments = enrollmentData?.Items?.ToList() ?? new List<EnrollmentDto>();

        Enrollments = allEnrollments
            .Where(x => string.IsNullOrWhiteSpace(FullName)
                || x.StudentName.Equals(FullName, StringComparison.OrdinalIgnoreCase)
                || x.StudentName.Contains(FullName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        long studentId = Enrollments.FirstOrDefault()?.StudentId ?? 0;

        // Với role Student, endpoint /students thường bị 403 nên ưu tiên lấy StudentId từ enrollments.
        if (studentId > 0)
        {
            DataSourceNote = "student-id: enrollments endpoint";
        }

        var studentIdFromTrustedSource = studentId > 0;

        var allItems = new List<TimetableItemDto>();

        if (studentIdFromTrustedSource)
        {
            allItems = await LoadStudentTimetableAsync(studentId);
            if (allItems.Any())
            {
                DataSourceNote = string.IsNullOrWhiteSpace(DataSourceNote)
                    ? "timetable: students/{id}/timetable"
                    : DataSourceNote + " | timetable: students/{id}/timetable";
            }
            else
            {
                DataSourceNote += " | students/{id}/timetable empty";
            }
        }
        else
        {
            DataSourceNote += " | không resolve được student-id từ enrollments (có thể do API trả 400/không có quyền)";
        }

        if (!allItems.Any() && Enrollments.Any())
        {
            allItems = await LoadClassTimetableFallbackAsync(Enrollments.Select(x => x.ClassId).Distinct().ToList());
            if (allItems.Any())
            {
                DataSourceNote += " | fallback: classes/{classId}/timetable";
            }
        }

        var weekStarts = allItems
            .Select(x => DateOnly.TryParse(x.SessionDate, out var d) ? d.AddDays(-(((int)d.DayOfWeek + 6) % 7)) : (DateOnly?)null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        if (!weekStarts.Contains(WeekStart))
        {
            weekStarts.Add(WeekStart);
            weekStarts = weekStarts.Distinct().OrderBy(x => x).ToList();
        }

        WeekOptions = weekStarts
            .Select(x => new WeekOptionVm
            {
                Value = x.ToString("yyyy-MM-dd"),
                Label = $"Tuần {x:dd/MM} - {x.AddDays(6):dd/MM}"
            })
            .ToList();

        Items = allItems.Where(x => DateOnly.TryParse(x.SessionDate, out var d) && d >= WeekStart && d <= WeekEnd).ToList();

        if (!Items.Any() && allItems.Any())
        {
            if (DateOnly.TryParse(allItems[0].SessionDate, out var firstDate))
            {
                WeekStart = firstDate.AddDays(-(((int)firstDate.DayOfWeek + 6) % 7));
                WeekEnd = WeekStart.AddDays(6);
                Items = allItems.Where(x => DateOnly.TryParse(x.SessionDate, out var d) && d >= WeekStart && d <= WeekEnd).ToList();
                DataSourceNote += " | week auto-shifted to first available session";
            }
        }

        if (!Items.Any())
        {
            DataSourceNote += " | chưa có buổi học từ API cho tài khoản hiện tại";
        }

        WeekDays = Enumerable.Range(0, 7).Select(x => WeekStart.AddDays(x)).ToList();
    }

    private async Task<List<TimetableItemDto>> LoadStudentTimetableAsync(long studentId)
    {
        var allUrl = $"students/{studentId}/timetable?PageNumber=1&PageSize=500&SortBy=SessionDate&SortDirection=asc";
        var allData = await _apiClient.GetAsync<PagedResult<TimetableItemDto>>(allUrl);
        return allData?.Items?.OrderBy(x => x.SessionDate).ThenBy(x => x.StartTime).ToList() ?? new List<TimetableItemDto>();
    }

    private async Task<List<TimetableItemDto>> LoadClassTimetableFallbackAsync(List<long> classIds)
    {
        var result = new List<TimetableItemDto>();

        foreach (var classId in classIds)
        {
            var url = $"classes/{classId}/timetable?PageNumber=1&PageSize=200&SortBy=SessionDate&SortDirection=asc";
            var data = await _apiClient.GetAsync<PagedResult<TimetableItemDto>>(url);
            if (data?.Items != null)
            {
                result.AddRange(data.Items);
            }
        }

        return result
            .GroupBy(x => x.SessionId)
            .Select(g => g.First())
            .OrderBy(x => x.SessionDate)
            .ThenBy(x => x.StartTime)
            .ToList();
    }

    public List<TimetableItemDto> DayItems(DateOnly day)
        => Items.Where(x => x.SessionDate == day.ToString("yyyy-MM-dd")).ToList();

    public int PositionTop(TimetableItemDto item)
    {
        if (!TimeOnly.TryParse(item.StartTime, out var st)) return 0;
        return Math.Max((st.Hour - 8) * 56 + (st.Minute * 56 / 60), 0);
    }

    public int ItemHeight(TimetableItemDto item)
    {
        if (!TimeOnly.TryParse(item.StartTime, out var st) || !TimeOnly.TryParse(item.EndTime, out var et)) return 56;
        var duration = (int)(et - st).TotalMinutes;
        return Math.Clamp(duration * 56 / 60, 48, 180);
    }

    public string DayLabel(DateOnly day) => day.DayOfWeek switch
    {
        DayOfWeek.Monday => "T2",
        DayOfWeek.Tuesday => "T3",
        DayOfWeek.Wednesday => "T4",
        DayOfWeek.Thursday => "T5",
        DayOfWeek.Friday => "T6",
        DayOfWeek.Saturday => "T7",
        _ => "CN"
    };


    public class WeekOptionVm
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class MonthOptionVm
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
