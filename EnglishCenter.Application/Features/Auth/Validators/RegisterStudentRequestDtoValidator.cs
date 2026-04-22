using EnglishCenter.Application.Features.Auth.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Auth.Validators;

public class RegisterStudentRequestDtoValidator : AbstractValidator<RegisterStudentRequestDto>
{
    public RegisterStudentRequestDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("UserName is required.")
            .MinimumLength(4).WithMessage("UserName must be at least 4 characters.")
            .MaximumLength(50).WithMessage("UserName must not exceed 50 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName is required.")
            .MaximumLength(255).WithMessage("FullName must not exceed 255 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("PhoneNumber must not exceed 20 characters.")
            .Matches("^[0-9+\\-\\s()]*$").When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("PhoneNumber format is invalid.");
    }
}
