using System.Text.Json;
using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.Admin.Users;

public class IndexModel : PageModel
{
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public bool IsCenterAdmin { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public long? EditId { get; set; }

    [BindProperty(SupportsGet = true)]
    public long? RoleUserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RoleCode { get; set; }

    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }

    public List<UserDto> Users { get; set; } = new();
    public UserDetailDto? EditingUser { get; set; }
    public UserDetailDto? RoleTargetUser { get; set; }
    public List<UserRoleDto> CurrentRoles { get; set; } = new();
    public Dictionary<long, string> UserRoleBadges { get; set; } = new();
    public List<RoleDto> AssignableRoles { get; set; } = new();

    [BindProperty]
    public CreateUserRequestDto CreateInput { get; set; } = new()
    {
        Status = 1
    };

    [BindProperty]
    public long? CreateRoleId { get; set; }

    [BindProperty]
    public UpdateUserInput UpdateInput { get; set; } = new();

    [BindProperty]
    public AssignRoleToUserRequestDto AssignInput { get; set; } = new();

    [BindProperty]
    public ReplaceUserRolesRequestDto ReplaceInput { get; set; } = new();

    [BindProperty]
    public string ReplaceRoleIdsText { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        if (!EnsureCenterAdmin())
        {
            return;
        }

        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!EnsureCenterAdmin())
        {
            return RedirectToPage();
        }

        await LoadAssignableRolesAsync();

        if (string.IsNullOrWhiteSpace(CreateInput.UserName)
            || string.IsNullOrWhiteSpace(CreateInput.PasswordHash)
            || string.IsNullOrWhiteSpace(CreateInput.FullName))
        {
            TempData["ErrorMessage"] = "UserName, Password, FullName are required.";
            return RedirectToPage();
        }

        if (!CreateRoleId.HasValue || CreateRoleId.Value <= 0)
        {
            TempData["ErrorMessage"] = "Role is required.";
            return RedirectToPage();
        }

        CreateInput.UserName = CreateInput.UserName.Trim();
        CreateInput.FullName = CreateInput.FullName.Trim();
        CreateInput.Email = string.IsNullOrWhiteSpace(CreateInput.Email) ? null : CreateInput.Email.Trim();
        CreateInput.PhoneNumber = string.IsNullOrWhiteSpace(CreateInput.PhoneNumber) ? null : CreateInput.PhoneNumber.Trim();
        CreateInput.RoleIds = new List<long> { CreateRoleId.Value };

        var created = await _apiClient.PostAsync<object,ApiCreateUserEnvelope>("campus-admin/users", new
        {
            userName = CreateInput.UserName,
            passwordHash = CreateInput.PasswordHash,
            email = CreateInput.Email,
            phoneNumber = CreateInput.PhoneNumber,
            fullName = CreateInput.FullName,
            status = CreateInput.Status,
            roleIds = CreateInput.RoleIds
        });

        var createdUserId = created?.Data?.Id ?? 0;
        if (createdUserId <= 0)
        {
            TempData["ErrorMessage"] = "Failed to create campus user.";
            return RedirectToPage();
        }

        var selectedRole = AssignableRoles.FirstOrDefault(x => x.Id == CreateRoleId.Value);
        var selectedRoleCode = selectedRole?.Code?.Trim().ToUpperInvariant();

        TempData["SuccessMessage"] = "Campus user created successfully.";

        if (selectedRoleCode == "STUDENT")
        {
            return RedirectToPage("./CreateStudentProfile", new { userId = createdUserId });
        }

        if (selectedRoleCode == "TEACHER")
        {
            return RedirectToPage("./CreateTeacherProfile", new { userId = createdUserId });
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (!EnsureCenterAdmin())
        {
            return RedirectToPage();
        }

        if (UpdateInput.Id <= 0 || string.IsNullOrWhiteSpace(UpdateInput.FullName))
        {
            TempData["ErrorMessage"] = "Invalid user update payload.";
            return RedirectToPage(new { EditId = UpdateInput.Id > 0 ? UpdateInput.Id : EditId });
        }

        UpdateInput.FullName = UpdateInput.FullName.Trim();
        UpdateInput.Email = string.IsNullOrWhiteSpace(UpdateInput.Email) ? null : UpdateInput.Email.Trim();
        UpdateInput.PhoneNumber = string.IsNullOrWhiteSpace(UpdateInput.PhoneNumber) ? null : UpdateInput.PhoneNumber.Trim();
        UpdateInput.PasswordHash = string.IsNullOrWhiteSpace(UpdateInput.PasswordHash) ? null : UpdateInput.PasswordHash;

        var ok = await _apiClient.PutAsync($"campus-admin/users/{UpdateInput.Id}", new
        {
            passwordHash = UpdateInput.PasswordHash,
            email = UpdateInput.Email,
            phoneNumber = UpdateInput.PhoneNumber,
            fullName = UpdateInput.FullName,
            status = UpdateInput.Status
        });

        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Campus user updated successfully."
            : "Failed to update campus user.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(long id)
    {
        if (!EnsureCenterAdmin())
        {
            return RedirectToPage();
        }

        var ok = await _apiClient.DeleteAsync($"campus-admin/users/{id}");
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Campus user deleted successfully."
            : "Failed to delete campus user.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAssignRoleAsync()
    {
        if (!EnsureCenterAdmin())
        {
            return RedirectToPage();
        }

        if (AssignInput.UserId <= 0 || AssignInput.RoleId <= 0)
        {
            TempData["ErrorMessage"] = "UserId and RoleId must be greater than 0.";
            return RedirectToPage(new { RoleUserId = AssignInput.UserId });
        }

        var ok = await _apiClient.PostAsync("campus-admin/user-roles/assign", new
        {
            userId = AssignInput.UserId,
            roleId = AssignInput.RoleId
        });

        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Role assigned successfully."
            : "Failed to assign role (check whitelist/campus scope).";

        return RedirectToPage(new { RoleUserId = AssignInput.UserId });
    }

    public async Task<IActionResult> OnPostRemoveRoleAsync(long removeUserId, long removeRoleId)
    {
        if (!EnsureCenterAdmin())
        {
            return RedirectToPage();
        }

        var ok = await _apiClient.DeleteAsync($"campus-admin/user-roles/{removeUserId}/{removeRoleId}");
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Role removed successfully."
            : "Failed to remove role.";

        return RedirectToPage(new { RoleUserId = removeUserId });
    }

    public async Task<IActionResult> OnPostReplaceRolesAsync()
    {
        if (!EnsureCenterAdmin())
        {
            return RedirectToPage();
        }

        if (ReplaceInput.UserId <= 0)
        {
            TempData["ErrorMessage"] = "Invalid user id for replace roles.";
            return RedirectToPage();
        }

        ReplaceInput.RoleIds = ParseRoleIds(ReplaceRoleIdsText);

        var ok = await _apiClient.PutAsync("campus-admin/user-roles/replace", new
        {
            userId = ReplaceInput.UserId,
            roleIds = ReplaceInput.RoleIds
        });

        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Roles replaced successfully."
            : "Failed to replace roles (check roleIds, whitelist, campus scope).";

        return RedirectToPage(new { RoleUserId = ReplaceInput.UserId });
    }

    private async Task LoadDataAsync()
    {
        await LoadAssignableRolesAsync();

        var userPaged = await _apiClient.GetAsync<PagedResult<UserDto>>($"campus-admin/users?pageNumber={PageNumber}&pageSize={PageSize}");

        Users = userPaged?.Items?.ToList() ?? new List<UserDto>();
        TotalPages = userPaged?.TotalPages ?? 1;
        TotalRecords = userPaged?.TotalRecords ?? 0;

        if (Users.Any())
        {
            var roleTasks = Users.Select(async u =>
            {
                var roles = await _apiClient.GetAsync<List<UserRoleDto>>($"campus-admin/user-roles/{u.Id}") ?? new List<UserRoleDto>();

                var roleLabel = roles
                    .Select(ResolveRoleDisplayName)
                    .Where(label => !string.IsNullOrWhiteSpace(label))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return (u.Id, Roles: roles, RoleLabel: roleLabel.Any() ? string.Join(", ", roleLabel) : "-");
            });

            var roleResults = await Task.WhenAll(roleTasks);
            UserRoleBadges = roleResults.ToDictionary(x => x.Id, x => x.RoleLabel);

            if (!string.IsNullOrWhiteSpace(RoleCode))
            {
                Users = roleResults
                    .Where(x => x.Roles.Any(r => string.Equals(r.RoleCode, RoleCode, StringComparison.OrdinalIgnoreCase)))
                    .Select(x => Users.First(u => u.Id == x.Id))
                    .ToList();

                TotalRecords = Users.Count;
                TotalPages = 1;
                PageNumber = 1;
            }
        }

        if (EditId.HasValue && EditId.Value > 0)
        {
            var detail = await _apiClient.GetAsync<UserDetailDto>($"campus-admin/users/{EditId.Value}");
            if (detail != null)
            {
                EditingUser = detail;
                UpdateInput = new UpdateUserInput
                {
                    Id = detail.Id,
                    Email = detail.Email,
                    PhoneNumber = detail.PhoneNumber,
                    FullName = detail.FullName,
                    Status = detail.Status
                };
            }
        }

        if (RoleUserId.HasValue && RoleUserId.Value > 0)
        {
            RoleTargetUser = await _apiClient.GetAsync<UserDetailDto>($"campus-admin/users/{RoleUserId.Value}");
            CurrentRoles = await _apiClient.GetAsync<List<UserRoleDto>>($"campus-admin/user-roles/{RoleUserId.Value}")
                ?? new List<UserRoleDto>();

            AssignInput.UserId = RoleUserId.Value;
            ReplaceInput.UserId = RoleUserId.Value;
            ReplaceRoleIdsText = string.Join(',', CurrentRoles.Select(x => x.RoleId));
        }
    }

    private bool EnsureCenterAdmin()
    {
        var rawRoles = HttpContext.Session.GetString("Roles");
        var roles = string.IsNullOrWhiteSpace(rawRoles)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(rawRoles) ?? new List<string>();

        IsCenterAdmin =
            roles.Contains("CENTER_ADMIN", StringComparer.OrdinalIgnoreCase)
            || roles.Contains("MANAGER", StringComparer.OrdinalIgnoreCase)
            || roles.Contains("ADMIN", StringComparer.OrdinalIgnoreCase);

        return IsCenterAdmin;
    }

    private static List<long> ParseRoleIds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new List<long>();

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => long.TryParse(x, out var id) ? id : 0)
            .Where(x => x > 0)
            .Distinct()
            .ToList();
    }

    private async Task LoadAssignableRolesAsync()
    {
        var rolePaged = await _apiClient.GetAsync<PagedResult<RoleDto>>("roles?pageNumber=1&pageSize=200");
        var roles = rolePaged?.Items?.ToList() ?? new List<RoleDto>();

        AssignableRoles = roles
            .Where(x => CampusAdminAssignableRoleCodes.Any(code => string.Equals(code, x.Code, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(x => x.Name)
            .ToList();
    }

    private static string ResolveRoleDisplayName(UserRoleDto role)
    {
        if (!string.IsNullOrWhiteSpace(role.RoleName)) return role.RoleName.Trim();
        if (!string.IsNullOrWhiteSpace(role.RoleCode)) return role.RoleCode.Trim();
        return string.Empty;
    }

    private static readonly string[] CampusAdminAssignableRoleCodes = ["STAFF", "TEACHER", "PARENT", "STUDENT"];

    public class ApiCreateUserEnvelope
    {
        public CreateUserResultDto? Data { get; set; }
    }

    public class CreateUserResultDto
    {
        public long Id { get; set; }
    }

    public class UpdateUserInput
    {
        public long Id { get; set; }
        public string? PasswordHash { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Status { get; set; } = 1;
    }
}