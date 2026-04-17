using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.RolePermissions.Dtos;

public class AssignPermissionToRoleRequestDto
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
}