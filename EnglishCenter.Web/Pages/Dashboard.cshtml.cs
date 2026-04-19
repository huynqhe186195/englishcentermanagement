using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishCenter.Web.Pages;

public class DashboardModel : PageModel
{
    private const string ApiClientName = "EnglishCenterApi";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DashboardModel> _logger;

    [BindProperty(SupportsGet = true)]
    public long? SelectedCampusId { get; set; }

    public string SelectedCampusName { get; private set; } = "Tất cả campus";
    public List<SelectListItem> CampusOptions { get; private set; } = new();
    public DashboardStatsViewModel Stats { get; private set; } = new();
    public IReadOnlyList<ClassDashboardItemViewModel> ClassDashboards { get; private set; } = Array.Empty<ClassDashboardItemViewModel>();
    public IReadOnlyList<ClassItemViewModel> Classes { get; private set; } = Array.Empty<ClassItemViewModel>();
    public string? ErrorMessage { get; private set; }

    public DashboardModel(IHttpClientFactory httpClientFactory, ILogger<DashboardModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        if (!SelectedCampusId.HasValue &&
            Request.Cookies.TryGetValue("ecm_campus_id", out var campusIdFromCookie) &&
            long.TryParse(campusIdFromCookie, out var parsedCampusId))
        {
            SelectedCampusId = parsedCampusId;
        }

        var client = _httpClientFactory.CreateClient(ApiClientName);

        try
        {
            var campusesTask = GetApiDataAsync<PagedResult<CampusItemViewModel>>(client, "/api/Campuses?pageNumber=1&pageSize=100");
            var classesTask = GetApiDataAsync<PagedResult<ClassItemViewModel>>(client, "/api/Classes?pageNumber=1&pageSize=100");
            var classDashboardTask = GetApiDataAsync<PagedResult<ClassDashboardItemViewModel>>(client, "/api/AcademicDashboard/class-dashboard?pageNumber=1&pageSize=100");

            await Task.WhenAll(campusesTask, classesTask, classDashboardTask);

            var campusesResult = await campusesTask;
            var classesResult = await classesTask;
            var classDashboardResult = await classDashboardTask;

            var campuses = campusesResult?.Items ?? Array.Empty<CampusItemViewModel>();
            var allClasses = classesResult?.Items ?? Array.Empty<ClassItemViewModel>();
            var allDashboards = classDashboardResult?.Items ?? Array.Empty<ClassDashboardItemViewModel>();

            CampusOptions = BuildCampusOptions(campuses, SelectedCampusId);
            SelectedCampusName = SelectedCampusId.HasValue
                ? campuses.FirstOrDefault(x => x.Id == SelectedCampusId.Value)?.Name ?? "Campus không tồn tại"
                : "Tất cả campus";

            Classes = SelectedCampusId.HasValue
                ? allClasses.Where(x => x.CampusId == SelectedCampusId.Value).ToList()
                : allClasses;

            var classCodes = Classes.Select(x => x.ClassCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
            ClassDashboards = allDashboards.Where(x => classCodes.Contains(x.ClassCode)).ToList();

            Stats = new DashboardStatsViewModel
            {
                TotalClasses = Classes.Count,
                ActiveClasses = Classes.Count(static x => x.Status == 1),
                AverageAttendanceRate = ClassDashboards.Count > 0
                    ? Math.Round(ClassDashboards.Average(static x => x.AttendanceRate), 2)
                    : 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải trang Dashboard từ API.");
            ErrorMessage = "Không tải được dữ liệu Dashboard. Hãy kiểm tra API hoặc Api:BaseUrl.";
        }
    }

    private static List<SelectListItem> BuildCampusOptions(IEnumerable<CampusItemViewModel> campuses, long? selectedCampusId)
    {
        var options = new List<SelectListItem>
        {
            new() { Value = string.Empty, Text = "Tất cả campus", Selected = !selectedCampusId.HasValue }
        };

        options.AddRange(campuses.Select(campus => new SelectListItem
        {
            Value = campus.Id.ToString(),
            Text = $"{campus.CampusCode} - {campus.Name}",
            Selected = selectedCampusId.HasValue && selectedCampusId == campus.Id
        }));

        return options;
    }

    private static async Task<T?> GetApiDataAsync<T>(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        return payload?.Success == true ? payload.Data : default;
    }

    public sealed class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
    }

    public sealed class CampusItemViewModel
    {
        public long Id { get; set; }
        public string CampusCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public sealed class ClassDashboardItemViewModel
    {
        public string ClassCode { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int MaxStudents { get; set; }
        public int ActiveEnrollments { get; set; }
        public int TotalSessions { get; set; }
        public int UpcomingSessions { get; set; }
        public decimal AttendanceRate { get; set; }
    }

    public sealed class ClassItemViewModel
    {
        public long Id { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public long? CampusId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TuitionFee { get; set; }
        public int MaxStudents { get; set; }
        public int Status { get; set; }
    }

    public sealed class DashboardStatsViewModel
    {
        public int TotalClasses { get; set; }
        public int ActiveClasses { get; set; }
        public decimal AverageAttendanceRate { get; set; }
    }
}
