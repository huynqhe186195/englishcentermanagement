namespace EnglishCenter.Web.Models;

public class UserDto
{
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Status { get; set; }
}

public class UserDetailDto
{
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UserRoleDto
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}


public class CreateUserRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Status { get; set; } = 1;
    public List<long> RoleIds { get; internal set; }
}

public class AssignRoleToUserRequestDto
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
}

public class ReplaceUserRolesRequestDto
{
    public long UserId { get; set; }
    public List<long> RoleIds { get; set; } = new();
}
