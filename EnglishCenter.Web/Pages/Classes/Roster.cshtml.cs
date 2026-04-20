using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Classes;

public class RosterModel : PageModel
{
    private readonly IApiClient _apiClient;

    public RosterModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<ClassRosterItemDto> Items { get; set; } = new();

    public async Task OnGetAsync(long id)
    {
        var data = await _apiClient.GetAsync<List<ClassRosterItemDto>>($"classes/{id}/roster");
        if (data != null) Items = data;
    }
}
