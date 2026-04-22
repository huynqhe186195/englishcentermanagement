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
            .Matches("^0\\d{0,9}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("PhoneNumber must start with 0, contain digits only, and be at most 10 digits.");
    }
}
