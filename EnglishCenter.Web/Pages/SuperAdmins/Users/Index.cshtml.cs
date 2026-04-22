using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.SuperAdmins.Users;

public class IndexModel : PageModel
{
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public long? RoleId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }

    public List<RoleDto> Roles { get; set; } = new();
    public List<CampusSimpleDto> Campuses { get; set; } = new();
    public List<UserListItemVm> Users { get; set; } = new();
    public long? AdminRoleId { get; set; }

    [BindProperty]
    public CreateAdminUserInput CreateInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        await EnsureAdminRoleIdAsync();

        if (!ModelState.IsValid || !AdminRoleId.HasValue)
        {
            await LoadDataAsync();
            return Page();
        }

        var ok = await _apiClient.PostAsync("users", new
        {
            userName = CreateInput.UserName.Trim(),
            passwordHash = CreateInput.Password.Trim(),
            email = string.IsNullOrWhiteSpace(CreateInput.Email) ? null : CreateInput.Email.Trim(),
            phoneNumber = string.IsNullOrWhiteSpace(CreateInput.PhoneNumber) ? null : CreateInput.PhoneNumber.Trim(),
            fullName = CreateInput.FullName.Trim(),
            status = CreateInput.Status,
            roleIds = new[] { AdminRoleId.Value }
        });

        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Admin user created successfully."
            : "Failed to create admin user.";

        return RedirectToPage(new { Keyword, RoleId, PageNumber });
    }

    private async Task LoadDataAsync()
    {
        var rolePaged = await _apiClient.GetAsync<PagedResult<RoleDto>>("roles?pageNumber=1&pageSize=200");
        Roles = rolePaged?.Items?.OrderBy(x => x.Name).ToList() ?? new List<RoleDto>();
        await EnsureAdminRoleIdAsync();

        var campusPaged = await _apiClient.GetAsync<PagedResult<CampusSimpleDto>>("campuses?pageNumber=1&pageSize=200");
        Campuses = campusPaged?.Items?.OrderBy(x => x.Name).ToList() ?? new List<CampusSimpleDto>();

        var url = $"users?pageNumber={PageNumber}&pageSize={PageSize}";
        if (!string.IsNullOrWhiteSpace(Keyword))
        {
            url += $"&keyword={Uri.EscapeDataString(Keyword.Trim())}";
        }

        var userPaged = await _apiClient.GetAsync<PagedResult<UserDto>>(url);
        var userItems = userPaged?.Items ?? new List<UserDto>();
        TotalPages = userPaged?.TotalPages ?? 1;
        TotalRecords = userPaged?.TotalRecords ?? 0;

        foreach (var user in userItems)
        {
            var roles = await _apiClient.GetAsync<List<UserRoleDto>>($"userroles/{user.Id}") ?? new List<UserRoleDto>();
            var detail = await _apiClient.GetAsync<UserDetailDto>($"users/{user.Id}");

            var primaryRole = roles.FirstOrDefault();

            Users.Add(new UserListItemVm
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                RoleName = primaryRole?.RoleName ?? "-",
                RoleId = primaryRole?.RoleId,
                Status = user.Status,
                CreatedAt = detail?.CreatedAt
            });
        }

        if (RoleId.HasValue)
        {
            Users = Users.Where(x => x.RoleId == RoleId.Value).ToList();
        }
    }

    private async Task EnsureAdminRoleIdAsync()
    {
        if (AdminRoleId.HasValue && AdminRoleId.Value > 0)
        {
            return;
        }

        if (!Roles.Any())
        {
            var rolePaged = await _apiClient.GetAsync<PagedResult<RoleDto>>("roles?pageNumber=1&pageSize=200");
            Roles = rolePaged?.Items?.OrderBy(x => x.Name).ToList() ?? new List<RoleDto>();
        }

        var adminRole = Roles.FirstOrDefault(x => string.Equals(x.Code, "ADMIN", StringComparison.OrdinalIgnoreCase));
        AdminRoleId = adminRole?.Id;

        if (!AdminRoleId.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Role ADMIN does not exist. Please seed roles before creating admin users.");
        }
    }

    public class UserListItemVm
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string RoleName { get; set; } = "-";
        public long? RoleId { get; set; }
        public int Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateAdminUserInput
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int Status { get; set; } = 1;
    }
}
