using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.SuperAdmins.Campuses;

public class IndexModel : PageModel
{
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<CampusRowVm> Campuses { get; set; } = new();
    public List<AdminLookupVm> AdminUsers { get; set; } = new();
    public CampusUpdateInput? EditingCampus { get; set; }

    [BindProperty(SupportsGet = true)]
    public long? EditId { get; set; }

    [BindProperty]
    public CampusCreateInput CreateInput { get; set; } = new();

    [BindProperty]
    public CampusUpdateInput UpdateInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        var ok = await _apiClient.PostAsync("campuses", new
        {
            campusCode = CreateInput.CampusCode.Trim(),
            name = CreateInput.Name.Trim(),
            address = string.IsNullOrWhiteSpace(CreateInput.Address) ? null : CreateInput.Address.Trim(),
            phone = string.IsNullOrWhiteSpace(CreateInput.Phone) ? null : CreateInput.Phone.Trim(),
            managerAdminUserId = CreateInput.ManagerAdminUserId,
            status = CreateInput.Status
        });

        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Campus created successfully."
            : "Failed to create campus.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (!ModelState.IsValid || UpdateInput.Id <= 0)
        {
            EditId = UpdateInput.Id > 0 ? UpdateInput.Id : EditId;
            await LoadDataAsync();
            return Page();
        }

        var ok = await _apiClient.PutAsync($"campuses/{UpdateInput.Id}", new
        {
            name = UpdateInput.Name.Trim(),
            address = string.IsNullOrWhiteSpace(UpdateInput.Address) ? null : UpdateInput.Address.Trim(),
            phone = string.IsNullOrWhiteSpace(UpdateInput.Phone) ? null : UpdateInput.Phone.Trim(),
            status = UpdateInput.Status
        });

        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Campus updated successfully."
            : "Failed to update campus.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(long id)
    {
        var ok = await _apiClient.DeleteAsync($"campuses/{id}");
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Campus deleted successfully."
            : "Failed to delete campus.";
        return RedirectToPage();
    }

    private async Task LoadDataAsync()
    {
        var campusPaged = await _apiClient.GetAsync<PagedResult<CampusDetailDto>>("campuses?pageNumber=1&pageSize=100");
        var campusRevenue = await _apiClient.GetAsync<List<RevenueByCampusItemDto>>("financialDashboard/revenue-by-campus")
            ?? new List<RevenueByCampusItemDto>();

        var campusItems = (campusPaged?.Items ?? new List<CampusDetailDto>()).ToList();
        if (!campusItems.Any() && campusRevenue.Any())
        {
            // Fallback: if campuses endpoint returns empty for current session/scope,
            // still render rows from revenue endpoint to avoid blank page.
            campusItems = campusRevenue
                .Select(x => new CampusDetailDto
                {
                    Id = x.CampusId,
                    CampusCode = x.CampusCode,
                    Name = x.CampusName,
                    Status = 1
                })
                .ToList();
        }

        foreach (var c in campusItems)
        {
            var revenue = campusRevenue.FirstOrDefault(x => x.CampusId == c.Id);
            Campuses.Add(new CampusRowVm
            {
                Id = c.Id,
                CampusName = c.Name,
                CampusCode = c.CampusCode,
                Address = c.Address,
                Phone = c.Phone,
                Status = c.Status,
                Revenue = revenue?.CollectedRevenue ?? 0,
                InvoiceCount = revenue?.InvoiceCount ?? 0
            });
        }

        await LoadAdminUsersAsync();

        if (EditId.HasValue && EditId.Value > 0)
        {
            var detail = await _apiClient.GetAsync<CampusDetailDto>($"campuses/{EditId.Value}");
            if (detail != null)
            {
                EditingCampus = new CampusUpdateInput
                {
                    Id = detail.Id,
                    CampusCode = detail.CampusCode,
                    Name = detail.Name,
                    Address = detail.Address,
                    Phone = detail.Phone,
                    Status = detail.Status
                };
                UpdateInput = EditingCampus;
            }
        }
    }

    public class CampusRowVm
    {
        public long Id { get; set; }
        public string CampusName { get; set; } = string.Empty;
        public string CampusCode { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public int Status { get; set; }
        public decimal Revenue { get; set; }
        public int InvoiceCount { get; set; }
    }

    public class CampusCreateInput
    {
        public string CampusCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public long? ManagerAdminUserId { get; set; }
        public int Status { get; set; } = 1;
    }

    public class AdminLookupVm
    {
        public long Id { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class CampusUpdateInput
    {
        public long Id { get; set; }
        public string CampusCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public int Status { get; set; } = 1;
    }

    private async Task LoadAdminUsersAsync()
    {
        AdminUsers = new List<AdminLookupVm>();

        var rolePaged = await _apiClient.GetAsync<PagedResult<RoleDto>>("roles?pageNumber=1&pageSize=200");
        var adminRole = rolePaged?.Items?.FirstOrDefault(x => string.Equals(x.Code, "ADMIN", StringComparison.OrdinalIgnoreCase));
        if (adminRole == null)
        {
            return;
        }

        var userPaged = await _apiClient.GetAsync<PagedResult<UserDto>>("users?pageNumber=1&pageSize=200");
        var users = userPaged?.Items ?? new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _apiClient.GetAsync<List<UserRoleDto>>($"userroles/{user.Id}") ?? new List<UserRoleDto>();
            var hasAdminRole = roles.Any(x => x.RoleId == adminRole.Id);
            if (!hasAdminRole || user.Status != 1 || user.CampusId.HasValue)
            {
                continue;
            }

            if (user.Id > 0)
            {
                AdminUsers.Add(new AdminLookupVm
                {
                    Id = user.Id,
                    Label = $"{user.FullName} ({user.UserName})"
                });
            }
        }
    }
}
