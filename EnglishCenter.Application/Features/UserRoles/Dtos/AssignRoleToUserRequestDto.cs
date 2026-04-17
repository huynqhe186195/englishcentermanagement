using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.UserRoles.Dtos;

public class AssignRoleToUserRequestDto
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
}
