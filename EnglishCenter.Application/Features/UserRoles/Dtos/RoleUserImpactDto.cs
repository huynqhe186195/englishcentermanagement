namespace EnglishCenter.Application.Features.UserRoles.Dtos;

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
