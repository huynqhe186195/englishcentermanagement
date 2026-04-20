using EnglishCenter.Application.Features.Classes.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Classes.Validators;

public class CreateClassRequestDtoValidator : AbstractValidator<CreateClassRequestDto>
{
    public CreateClassRequestDtoValidator()
    {
        RuleFor(x => x.ClassCode)
            .NotEmpty().WithMessage("ClassCode is required.")
            .MaximumLength(50).WithMessage("ClassCode must not exceed 50 characters.");

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
        RuleFor(x => x.CampusId)
    .GreaterThan(0).WithMessage("CampusId must be greater than 0.");
    }
}
