using EnglishCenter.Application.Features.ClassSchedules.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.ClassSchedules.Validators;

public class CreateClassScheduleRequestDtoValidator : AbstractValidator<CreateClassScheduleRequestDto>
{
    public CreateClassScheduleRequestDtoValidator()
    {
        RuleFor(x => x.ClassId)
            .GreaterThan(0).WithMessage("ClassId must be greater than 0.");

        RuleFor(x => x.DayOfWeek)
            .InclusiveBetween(1, 7).WithMessage("DayOfWeek must be between 1 and 7.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("StartTime is required.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("EndTime is required.");

        RuleFor(x => x)
            .Must(x => x.StartTime < x.EndTime)
            .WithMessage("StartTime must be less than EndTime.");

        RuleFor(x => x.RoomId)
            .GreaterThan(0)
            .When(x => x.RoomId.HasValue)
            .WithMessage("RoomId must be greater than 0.");
    }
}