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

    public List<RoleDto> Roles { get; set; } = new();
    public List<CampusSimpleDto> Campuses { get; set; } = new();
    public List<UserListItemVm> Users { get; set; } = new();

    public async Task OnGetAsync()
    {
        var rolePaged = await _apiClient.GetAsync<PagedResult<RoleDto>>("roles?pageNumber=1&pageSize=200");
        Roles = rolePaged?.Items?.OrderBy(x => x.Name).ToList() ?? new List<RoleDto>();

        var campusPaged = await _apiClient.GetAsync<PagedResult<CampusSimpleDto>>("campuses?pageNumber=1&pageSize=200");
        Campuses = campusPaged?.Items?.OrderBy(x => x.Name).ToList() ?? new List<CampusSimpleDto>();

        var url = $"users?pageNumber=1&pageSize=100";
        if (!string.IsNullOrWhiteSpace(Keyword))
        {
            url += $"&keyword={Uri.EscapeDataString(Keyword.Trim())}";
        }

        var userPaged = await _apiClient.GetAsync<PagedResult<UserDto>>(url);
        var userItems = userPaged?.Items ?? new List<UserDto>();

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
}
