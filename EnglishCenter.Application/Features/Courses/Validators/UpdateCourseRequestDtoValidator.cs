using EnglishCenter.Application.Features.Courses.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Courses.Validators;

public class UpdateCourseRequestDtoValidator : AbstractValidator<UpdateCourseRequestDto>
{
    public UpdateCourseRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.Level)
            .MaximumLength(100).WithMessage("Level must not exceed 100 characters.");

        RuleFor(x => x.TotalSessions)
            .GreaterThan(0).WithMessage("TotalSessions must be greater than 0.");

        RuleFor(x => x.DefaultFee)
            .GreaterThanOrEqualTo(0).WithMessage("DefaultFee must be greater than or equal to 0.");

        RuleFor(x => x.Status)
            .InclusiveBetween(0, 1).WithMessage("Status must be 0 or 1.");

        RuleFor(x => x)
            .Must(x => !x.AgeMin.HasValue || !x.AgeMax.HasValue || x.AgeMin <= x.AgeMax)
            .WithMessage("AgeMin must be less than or equal to AgeMax.");
    }
}