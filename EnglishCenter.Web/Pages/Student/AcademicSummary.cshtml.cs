using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Student;

public class AcademicSummaryModel : PageModel
{
    private readonly IApiClient _apiClient;

    public AcademicSummaryModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public StudentAcademicSummaryDto Summary { get; set; } = new();

    public async Task OnGetAsync(long studentId)
    {
        var data = await _apiClient.GetAsync<StudentAcademicSummaryDto>($"students/{studentId}/academic-summary");
        if (data != null) Summary = data;
    }
}
