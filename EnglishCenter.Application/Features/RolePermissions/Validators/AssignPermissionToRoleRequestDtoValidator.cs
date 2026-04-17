using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.RolePermissions.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.RolePermissions.Validators;

public class AssignPermissionToRoleRequestDtoValidator : AbstractValidator<AssignPermissionToRoleRequestDto>
{
    public AssignPermissionToRoleRequestDtoValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("RoleId must be greater than 0.");

        RuleFor(x => x.PermissionId)
            .GreaterThan(0).WithMessage("PermissionId must be greater than 0.");
    }
}
