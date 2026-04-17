using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.RolePermissions.Dtos;

public class ReplaceRolePermissionsRequestDto
{
    public long RoleId { get; set; }
    public List<long> PermissionIds { get; set; } = [];
}
