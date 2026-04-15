using EnglishCenter.Application.Features.Students.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Students.Validators;

public class CreateStudentRequestDtoValidator : AbstractValidator<CreateStudentRequestDto>
{
    public CreateStudentRequestDtoValidator()
    {
        RuleFor(x => x.StudentCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("StudentCode is required.")
            .Must(code => !string.IsNullOrWhiteSpace(code)).WithMessage("StudentCode is required.")
            .MinimumLength(3).WithMessage("StudentCode must be at least 3 characters.")
            .MaximumLength(50).WithMessage("StudentCode must not exceed 50 characters.");

        RuleFor(x => x.FullName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("FullName is required.")
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("FullName is required.")
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

        RuleFor(x => x.SchoolName)
            .MaximumLength(255).WithMessage("SchoolName must not exceed 255 characters.");

        RuleFor(x => x.EnglishLevel)
            .MaximumLength(100).WithMessage("EnglishLevel must not exceed 100 characters.");

        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.");

        RuleFor(x => x.Status)
            .InclusiveBetween(0, 1).WithMessage("Status must be 0 or 1.");

        RuleFor(x => x.DateOfBirth)
            .Must(d => d == null || d <= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("DateOfBirth cannot be in the future.");
    }
}
