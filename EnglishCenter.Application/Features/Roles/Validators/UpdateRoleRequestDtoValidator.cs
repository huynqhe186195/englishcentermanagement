using EnglishCenter.Application.Features.Roles.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Roles.Validators;

public class UpdateRoleRequestDtoValidator : AbstractValidator<UpdateRoleRequestDto>
{
    public UpdateRoleRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}
