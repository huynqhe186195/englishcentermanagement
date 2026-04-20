using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Classes;

public class DetailsModel : PageModel
{
    private readonly IApiClient _apiClient;

    public DetailsModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ClassDetailDto Item { get; set; } = new();

    public async Task OnGetAsync(long id)
    {
        var data = await _apiClient.GetAsync<ClassDetailDto>($"classes/{id}");
        if (data != null) Item = data;
    }
}
