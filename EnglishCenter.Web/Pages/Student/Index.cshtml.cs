using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;
using System.Text.Json;

namespace EnglishCenter.Web.Pages.Student;

public class IndexModel : PageModel
{
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();

    public async Task OnGetAsync()
    {
        // attempt to call auth/me
        var me = await _apiClient.GetAsync<EnglishCenter.Web.Models.CurrentUserDto>("auth/me");
        if (me != null)
        {
            UserName = me.UserName;
            FullName = me.FullName;
            Roles = me.Roles ?? new List<string>();
        }
    }
}
