using EnglishCenter.Application.Features.Users.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Users.Validators;

public class CreateUserRequestDtoValidator : AbstractValidator<CreateUserRequestDto>
{
    public CreateUserRequestDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("UserName is required.")
            .MaximumLength(100).WithMessage("UserName must not exceed 100 characters.");

        RuleFor(x => x.PasswordHash)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

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
