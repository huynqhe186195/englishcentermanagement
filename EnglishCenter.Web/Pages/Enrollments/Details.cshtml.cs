using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Enrollments;

public class DetailsModel : PageModel
{
    private readonly IApiClient _apiClient;

    public DetailsModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public EnrollmentDetailDto Item { get; set; } = new();

    public async Task OnGetAsync(long id)
    {
        var data = await _apiClient.GetAsync<EnrollmentDetailDto>($"enrollments/{id}");
        if (data != null) Item = data;
    }
}
