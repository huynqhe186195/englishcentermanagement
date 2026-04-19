using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages;

public class HomeModel : PageModel
{
    private const string ApiClientName = "EnglishCenterApi";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HomeModel> _logger;

    public string? ErrorMessage { get; private set; }
    public IReadOnlyList<CourseCardViewModel> Courses { get; private set; } = Array.Empty<CourseCardViewModel>();

    public HomeModel(IHttpClientFactory httpClientFactory, ILogger<HomeModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient(ApiClientName);

        try
        {
            var classesResult = await GetApiDataAsync<PagedResult<ClassItemViewModel>>(client, "/api/Classes?pageNumber=1&pageSize=12");
            var classes = classesResult?.Items ?? Array.Empty<ClassItemViewModel>();

            Courses = classes
                .OrderBy(x => x.StartDate)
                .Select(x => new CourseCardViewModel
                {
                    Id = x.Id,
                    ClassCode = x.ClassCode,
                    Title = x.Name,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    TuitionFee = x.TuitionFee,
                    MaxStudents = x.MaxStudents,
                    Status = x.Status
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải danh sách khóa học trang Home.");
            ErrorMessage = "Không thể tải danh sách khóa học. Vui lòng kiểm tra API.";
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

    public sealed class CourseCardViewModel
    {
        public long Id { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal TuitionFee { get; set; }
        public int MaxStudents { get; set; }
        public int Status { get; set; }
    }
}
