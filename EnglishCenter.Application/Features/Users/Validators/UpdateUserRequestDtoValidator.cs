using EnglishCenter.Application.Features.Users.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Users.Validators;

public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserRequestDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName is required.")
            .MaximumLength(255).WithMessage("FullName must not exceed 255 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("Email format is invalid.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("PhoneNumber must not exceed 20 characters.");
    }
}
