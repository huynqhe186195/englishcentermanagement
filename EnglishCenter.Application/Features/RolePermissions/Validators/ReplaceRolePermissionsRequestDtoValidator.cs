using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.RolePermissions.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.RolePermissions.Validators;

public class ReplaceRolePermissionsRequestDtoValidator : AbstractValidator<ReplaceRolePermissionsRequestDto>
{
    public ReplaceRolePermissionsRequestDtoValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("RoleId must be greater than 0.");

        RuleFor(x => x.PermissionIds)
            .NotNull().WithMessage("PermissionIds is required.");

        RuleForEach(x => x.PermissionIds)
            .GreaterThan(0).WithMessage("Each PermissionId must be greater than 0.");
    }
}
