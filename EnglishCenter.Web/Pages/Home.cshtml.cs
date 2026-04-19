using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages;

public class HomeModel : PageModel
{
    private const string ApiClientName = "EnglishCenterApi";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HomeModel> _logger;

    [BindProperty(SupportsGet = true)]
    public long? CampusId { get; set; }

    public string? ErrorMessage { get; private set; }
    public IReadOnlyList<CampusCardViewModel> Campuses { get; private set; } = Array.Empty<CampusCardViewModel>();
    public CampusCardViewModel? SelectedCampus { get; private set; }

    public HomeModel(IHttpClientFactory httpClientFactory, ILogger<HomeModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        if (!CampusId.HasValue &&
            Request.Cookies.TryGetValue("ecm_campus_id", out var campusCookie) &&
            long.TryParse(campusCookie, out var parsedCampusId))
        {
            CampusId = parsedCampusId;
        }

        var client = _httpClientFactory.CreateClient(ApiClientName);

        try
        {
            var campusesResult = await GetApiDataAsync<PagedResult<CampusCardViewModel>>(client, "/api/Campuses?pageNumber=1&pageSize=100");
            Campuses = campusesResult?.Items ?? Array.Empty<CampusCardViewModel>();

            if (CampusId.HasValue)
            {
                SelectedCampus = Campuses.FirstOrDefault(x => x.Id == CampusId.Value);
            }

            if (SelectedCampus is null && Campuses.Count > 0)
            {
                SelectedCampus = Campuses[0];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải trang Home.");
            ErrorMessage = "Không thể tải danh sách campus. Vui lòng kiểm tra API.";
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

    public sealed class CampusCardViewModel
    {
        public long Id { get; set; }
        public string CampusCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public int Status { get; set; }
    }
}
