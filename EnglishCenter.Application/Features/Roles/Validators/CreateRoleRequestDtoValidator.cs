using EnglishCenter.Application.Features.Roles.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Roles.Validators;

public class CreateRoleRequestDtoValidator : AbstractValidator<CreateRoleRequestDto>
{
    public CreateRoleRequestDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}
