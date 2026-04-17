using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.UserRoles.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.UserRoles.Validators;

public class AssignRoleToUserRequestDtoValidator : AbstractValidator<AssignRoleToUserRequestDto>
{
    public AssignRoleToUserRequestDtoValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0.");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("RoleId must be greater than 0.");
    }
}
