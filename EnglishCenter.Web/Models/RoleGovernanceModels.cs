namespace EnglishCenter.Web.Models;

public class RoleDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class RolePermissionDto
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
}

public class ReplaceRolePermissionsRequestDto
{
    public long RoleId { get; set; }
    public List<long> PermissionIds { get; set; } = new();
}

public class RoleUserImpactDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class RoleUserImpactResultDto
{
    public long RoleId { get; set; }
    public int TotalUsers { get; set; }
    public List<RoleUserImpactDto> Users { get; set; } = new();
}
