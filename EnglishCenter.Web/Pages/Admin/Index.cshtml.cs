using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;
using Microsoft.AspNetCore.Mvc;

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
    public List<RevenueByCampusItemDto> RevenueByCampus { get; set; } = new();
    public List<ClassDashboardByCampusItemDto> ClassByCampus { get; set; } = new();
    public List<TeacherWorkloadByCampusItemDto> TeacherByCampus { get; set; } = new();
    public List<RoomUtilizationByCampusItemDto> RoomByCampus { get; set; } = new();
    public RevenueSummaryDto RevenueSummary { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public long? CampusId { get; set; }

    public RevenueByCampusItemDto? SelectedRevenueCampus { get; set; }
    public ClassDashboardByCampusItemDto? SelectedClassCampus { get; set; }
    public TeacherWorkloadByCampusItemDto? SelectedTeacherCampus { get; set; }
    public RoomUtilizationByCampusItemDto? SelectedRoomCampus { get; set; }
    public string DrilldownTitle => CampusId.HasValue
        ? $"Drill-down campus #{CampusId}"
        : "Global dashboard (all campuses)";

    public int TotalClasses { get; set; }
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalEnrollments { get; set; }

    public bool IsSuperAdmin { get; set; }
    public bool IsCenterAdmin { get; set; }
    public bool HasGlobalDashboardAccess { get; set; }

    public async Task OnGetAsync()
    {
        ResolveRoleFlags();

        HasGlobalDashboardAccess = IsSuperAdmin;

        if (HasGlobalDashboardAccess)
        {
            await LoadGlobalDashboardsAsync();
        }

        await LoadCountCardsAsync();
    }

    private async Task LoadGlobalDashboardsAsync()
    {
        var classesData = await _apiClient.GetAsync<PagedResult<ClassDashboardDto>>("academicDashboard/class-dashboard?PageNumber=1&PageSize=5");
        if (classesData != null) TopClasses = classesData.Items;

        var atRisk = await _apiClient.GetAsync<PagedResult<StudentAtRiskDto>>("academicDashboard/students-at-risk?PageNumber=1&PageSize=5&AttendanceThreshold=80");
        if (atRisk != null) StudentsAtRisk = atRisk.Items;

        var roomsData = await _apiClient.GetAsync<PagedResult<RoomUtilizationDto>>("academicDashboard/room-utilization?PageNumber=1&PageSize=5");
        if (roomsData != null) Rooms = roomsData.Items;

        RevenueSummary = await _apiClient.GetAsync<RevenueSummaryDto>("financialDashboard/revenue-summary") ?? new RevenueSummaryDto();
        RevenueByCampus = await _apiClient.GetAsync<List<RevenueByCampusItemDto>>("financialDashboard/revenue-by-campus") ?? new List<RevenueByCampusItemDto>();
        ClassByCampus = await _apiClient.GetAsync<List<ClassDashboardByCampusItemDto>>("financialDashboard/class-dashboard-by-campus") ?? new List<ClassDashboardByCampusItemDto>();
        TeacherByCampus = await _apiClient.GetAsync<List<TeacherWorkloadByCampusItemDto>>("financialDashboard/teacher-workload-by-campus") ?? new List<TeacherWorkloadByCampusItemDto>();
        RoomByCampus = await _apiClient.GetAsync<List<RoomUtilizationByCampusItemDto>>("financialDashboard/room-utilization-by-campus") ?? new List<RoomUtilizationByCampusItemDto>();

        if (CampusId.HasValue)
        {
            SelectedRevenueCampus = RevenueByCampus.FirstOrDefault(x => x.CampusId == CampusId.Value);
            SelectedClassCampus = ClassByCampus.FirstOrDefault(x => x.CampusId == CampusId.Value);
            SelectedTeacherCampus = TeacherByCampus.FirstOrDefault(x => x.CampusId == CampusId.Value);
            SelectedRoomCampus = RoomByCampus.FirstOrDefault(x => x.CampusId == CampusId.Value);
        }
        else
        {
            SelectedRevenueCampus = RevenueByCampus.OrderByDescending(x => x.CollectedRevenue).FirstOrDefault();
            SelectedClassCampus = ClassByCampus.OrderByDescending(x => x.ActiveEnrollments).FirstOrDefault();
            SelectedTeacherCampus = TeacherByCampus.OrderByDescending(x => x.TeacherCount).FirstOrDefault();
            SelectedRoomCampus = RoomByCampus.OrderByDescending(x => x.RoomCount).FirstOrDefault();
        }
    }

    private async Task LoadCountCardsAsync()
    {
        var classesCount = await _apiClient.GetAsync<PagedResult<object>>("classes?PageNumber=1&PageSize=1");
        TotalClasses = classesCount?.TotalRecords ?? 0;

        if (IsSuperAdmin)
        {
            var studentsCount = await _apiClient.GetAsync<PagedResult<object>>("students?PageNumber=1&PageSize=1");
            TotalStudents = studentsCount?.TotalRecords ?? 0;
        }
        else if (IsCenterAdmin)
        {
            var campusUsers = await _apiClient.GetAsync<PagedResult<UserDto>>("campus-admin/users?pageNumber=1&pageSize=1");
            TotalStudents = campusUsers?.TotalRecords ?? 0;
        }

        var teachersCount = await _apiClient.GetAsync<PagedResult<object>>("teachers?PageNumber=1&PageSize=1");
        TotalTeachers = teachersCount?.TotalRecords ?? 0;

        var enrollmentsCount = await _apiClient.GetAsync<PagedResult<object>>("enrollments?PageNumber=1&PageSize=1");
        TotalEnrollments = enrollmentsCount?.TotalRecords ?? 0;
    }

    private void ResolveRoleFlags()
    {
        var rawRoles = HttpContext.Session.GetString("Roles");
        var roles = string.IsNullOrWhiteSpace(rawRoles)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(rawRoles) ?? new List<string>();

        IsSuperAdmin = roles.Contains("SUPER_ADMIN", StringComparer.OrdinalIgnoreCase);
        IsCenterAdmin =
            roles.Contains("CENTER_ADMIN", StringComparer.OrdinalIgnoreCase)
            || roles.Contains("MANAGER", StringComparer.OrdinalIgnoreCase)
            || roles.Contains("ADMIN", StringComparer.OrdinalIgnoreCase);
    }
}
