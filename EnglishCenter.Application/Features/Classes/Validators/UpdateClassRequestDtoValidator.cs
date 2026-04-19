using EnglishCenter.Application.Features.Classes.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Classes.Validators;

public class UpdateClassRequestDtoValidator : AbstractValidator<UpdateClassRequestDto>
{
    public UpdateClassRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters.");

        RuleFor(x => x.TuitionFee)
            .GreaterThanOrEqualTo(0).WithMessage("TuitionFee must be greater than or equal to 0.");

        RuleFor(x => x.MaxStudents)
            .GreaterThan(0).WithMessage("MaxStudents must be greater than 0.");

        RuleFor(x => x.Status)
            .InclusiveBetween(0, 1).WithMessage("Status must be 0 or 1.");

        RuleFor(x => x.EndDate)
            .Must((dto, endDate) => endDate > dto.StartDate)
            .WithMessage("EndDate must be greater than StartDate.");

        RuleFor(x => x.MaxStudents)
            .Equal(10)
            .WithMessage("Each class can only have 10 students.");
    }
}
