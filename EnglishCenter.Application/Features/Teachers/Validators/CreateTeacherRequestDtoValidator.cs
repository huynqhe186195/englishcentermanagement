using EnglishCenter.Application.Features.Teachers.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Teachers.Validators;

public class CreateTeacherRequestDtoValidator : AbstractValidator<CreateTeacherRequestDto>
{
    public CreateTeacherRequestDtoValidator()
    {
        RuleFor(x => x.TeacherCode)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.TeacherCode)).WithMessage("TeacherCode must not exceed 50 characters.")
            .Must(code => code == null || !string.IsNullOrWhiteSpace(code)).WithMessage("TeacherCode cannot be whitespace.");

        RuleFor(x => x.FullName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("FullName is required.")
            .MaximumLength(255).WithMessage("FullName must not exceed 255 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.")
            .Matches(@"^[0-9+\-\s()]*$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Phone number format is invalid.");

        RuleFor(x => x.Email)
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Email format is invalid.");

        RuleFor(x => x.Status)
            .InclusiveBetween(0,1).WithMessage("Status must be 0 or 1.");
    }
}
