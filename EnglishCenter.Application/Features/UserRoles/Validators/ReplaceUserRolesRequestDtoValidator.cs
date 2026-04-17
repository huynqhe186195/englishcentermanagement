using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.UserRoles.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.UserRoles.Validators;

public class ReplaceUserRolesRequestDtoValidator : AbstractValidator<ReplaceUserRolesRequestDto>
{
    public ReplaceUserRolesRequestDtoValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0.");

        RuleFor(x => x.RoleIds)
            .NotNull().WithMessage("RoleIds is required.");

        RuleForEach(x => x.RoleIds)
            .GreaterThan(0).WithMessage("Each RoleId must be greater than 0.");
    }
}
