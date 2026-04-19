using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IEnumerable<ClassDashboardDto> TopClasses { get; set; } = Enumerable.Empty<ClassDashboardDto>();
    public IEnumerable<StudentAtRiskDto> StudentsAtRisk { get; set; } = Enumerable.Empty<StudentAtRiskDto>();
    public IEnumerable<RoomUtilizationDto> Rooms { get; set; } = Enumerable.Empty<RoomUtilizationDto>();

    public int TotalClasses { get; set; }
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalEnrollments { get; set; }

    public async Task OnGetAsync()
    {
        var classesData = await _apiClient.GetAsync<PagedResult<ClassDashboardDto>>("academicDashboard/class-dashboard?PageNumber=1&PageSize=5");
        if (classesData != null) TopClasses = classesData.Items;

        var atRisk = await _apiClient.GetAsync<PagedResult<StudentAtRiskDto>>("academicDashboard/students-at-risk?PageNumber=1&PageSize=5&AttendanceThreshold=80");
        if (atRisk != null) StudentsAtRisk = atRisk.Items;

        var roomsData = await _apiClient.GetAsync<PagedResult<RoomUtilizationDto>>("academicDashboard/room-utilization?PageNumber=1&PageSize=5");
        if (roomsData != null) Rooms = roomsData.Items;

        // counts: call paged endpoints with PageSize=1 and read TotalRecords
        var classesCount = await _apiClient.GetAsync<PagedResult<object>>("classes?PageNumber=1&PageSize=1");
        TotalClasses = classesCount?.TotalRecords ?? 0;
        var studentsCount = await _apiClient.GetAsync<PagedResult<object>>("students?PageNumber=1&PageSize=1");
        TotalStudents = studentsCount?.TotalRecords ?? 0;
        var teachersCount = await _apiClient.GetAsync<PagedResult<object>>("teachers?PageNumber=1&PageSize=1");
        TotalTeachers = teachersCount?.TotalRecords ?? 0;
        var enrollmentsCount = await _apiClient.GetAsync<PagedResult<object>>("enrollments?PageNumber=1&PageSize=1");
        TotalEnrollments = enrollmentsCount?.TotalRecords ?? 0;
    }
}
