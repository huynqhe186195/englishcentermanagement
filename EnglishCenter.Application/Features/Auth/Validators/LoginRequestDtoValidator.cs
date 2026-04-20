using EnglishCenter.Application.Features.Auth.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Auth.Validators;

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("UserName is required.")
            .MaximumLength(100).WithMessage("UserName must not exceed 100 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.CampusId)
            .GreaterThan(0).WithMessage("Campus is required.");
    }
}
