using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages;

public class IndexModel : PageModel
{
    private const string ApiClientName = "EnglishCenterApi";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IndexModel> _logger;

    public DashboardStatsViewModel Stats { get; private set; } = new();
    public IReadOnlyList<ClassDashboardItemViewModel> ClassDashboards { get; private set; } = Array.Empty<ClassDashboardItemViewModel>();
    public IReadOnlyList<ClassItemViewModel> Classes { get; private set; } = Array.Empty<ClassItemViewModel>();
    public string? ErrorMessage { get; private set; }

    public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient(ApiClientName);

        try
        {
            var classDashboardTask = GetApiDataAsync<PagedResult<ClassDashboardItemViewModel>>(client, "/api/AcademicDashboard/class-dashboard?pageNumber=1&pageSize=6");
            var classesTask = GetApiDataAsync<PagedResult<ClassItemViewModel>>(client, "/api/Classes?pageNumber=1&pageSize=8");
            var studentsAtRiskTask = GetApiDataAsync<PagedResult<StudentAtRiskViewModel>>(client, "/api/AcademicDashboard/students-at-risk?pageNumber=1&pageSize=50");

            await Task.WhenAll(classDashboardTask, classesTask, studentsAtRiskTask);

            var classDashboardResult = await classDashboardTask;
            var classesResult = await classesTask;
            var studentsAtRiskResult = await studentsAtRiskTask;

            ClassDashboards = classDashboardResult?.Items ?? Array.Empty<ClassDashboardItemViewModel>();
            Classes = classesResult?.Items ?? Array.Empty<ClassItemViewModel>();

            Stats = new DashboardStatsViewModel
            {
                TotalClasses = classesResult?.TotalRecords ?? 0,
                ActiveClasses = Classes.Count(static c => c.Status == 1),
                AverageAttendanceRate = ClassDashboards.Count > 0
                    ? Math.Round(ClassDashboards.Average(static x => x.AttendanceRate), 2)
                    : 0,
                StudentsAtRisk = studentsAtRiskResult?.TotalRecords ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải dữ liệu dashboard từ API.");
            ErrorMessage = "Không thể tải dữ liệu từ API. Vui lòng kiểm tra EnglishCenter.Api đã chạy đúng cổng cấu hình.";
        }
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

    public sealed class ClassDashboardItemViewModel
    {
        public long ClassId { get; set; }
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
        public string Name { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal TuitionFee { get; set; }
        public int MaxStudents { get; set; }
        public int Status { get; set; }
    }

    public sealed class StudentAtRiskViewModel
    {
        public long StudentId { get; set; }
    }

    public sealed class DashboardStatsViewModel
    {
        public int TotalClasses { get; set; }
        public int ActiveClasses { get; set; }
        public decimal AverageAttendanceRate { get; set; }
        public int StudentsAtRisk { get; set; }
    }
}
