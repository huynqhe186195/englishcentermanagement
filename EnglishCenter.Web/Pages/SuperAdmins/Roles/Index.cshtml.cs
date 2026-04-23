using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.SuperAdmins.Roles;

public class IndexModel : PageModel
{
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty(SupportsGet = true)]
    public long? RoleId { get; set; }

    [BindProperty]
    public List<long> SelectedPermissionIds { get; set; } = new();

    public List<RoleDto> Roles { get; set; } = new();
    public List<RolePermissionDto> CurrentPermissions { get; set; } = new();
    public List<RolePermissionDto> CatalogPermissions { get; set; } = new();
    public RoleUserImpactResultDto Impact { get; set; } = new();
    public string? SaveMessage { get; set; }
    public bool SaveSuccess { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!RoleId.HasValue || RoleId.Value <= 0)
        {
            SaveSuccess = false;
            SaveMessage = "Role không hợp lệ.";
            await LoadAsync();
            return Page();
        }

        var requestedPermissionIds = SelectedPermissionIds
            .Distinct()
            .ToList();

        var ok = await _apiClient.PutAsync("rolepermissions/replace", new ReplaceRolePermissionsRequestDto
        {
            RoleId = RoleId.Value,
            PermissionIds = requestedPermissionIds
        });

        SaveSuccess = ok;
        SaveMessage = ok
            ? "Đã lưu thay đổi permissions."
            : "Lưu permissions thất bại. Vui lòng kiểm tra quyền hoặc dữ liệu.";

        await LoadAsync();
        return Page();
    }

    private async Task LoadAsync()
    {
        var rolePaged = await _apiClient.GetAsync<PagedResult<RoleDto>>("roles?pageNumber=1&pageSize=200");
        Roles = rolePaged?.Items?.OrderBy(x => x.Name).ToList() ?? new List<RoleDto>();

        if (!RoleId.HasValue || Roles.All(x => x.Id != RoleId.Value))
        {
            RoleId = Roles.FirstOrDefault()?.Id;
        }

        if (!RoleId.HasValue || RoleId.Value <= 0)
        {
            CurrentPermissions = new List<RolePermissionDto>();
            CatalogPermissions = new List<RolePermissionDto>();
            Impact = new RoleUserImpactResultDto();
            return;
        }

        CurrentPermissions = await _apiClient.GetAsync<List<RolePermissionDto>>($"rolepermissions/{RoleId.Value}")
            ?? new List<RolePermissionDto>();
        SelectedPermissionIds = CurrentPermissions.Select(x => x.PermissionId).Distinct().ToList();

        // Build catalog from all role permissions (fallback when permission master endpoint is not available).
        var catalog = new Dictionary<long, RolePermissionDto>();
        foreach (var role in Roles)
        {
            var perms = await _apiClient.GetAsync<List<RolePermissionDto>>($"rolepermissions/{role.Id}") ?? new List<RolePermissionDto>();
            foreach (var p in perms)
            {
                catalog[p.PermissionId] = p;
            }
        }

        CatalogPermissions = catalog.Values
            .OrderBy(x => x.PermissionCode)
            .ToList();

        Impact = await _apiClient.GetAsync<RoleUserImpactResultDto>($"userroles/role/{RoleId.Value}?pageNumber=1&pageSize=8")
            ?? new RoleUserImpactResultDto { RoleId = RoleId.Value };
    }
}
