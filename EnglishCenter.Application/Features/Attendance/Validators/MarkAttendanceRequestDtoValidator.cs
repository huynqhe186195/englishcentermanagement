using EnglishCenter.Application.Features.Attendance.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Attendance.Validators;

public class MarkAttendanceRequestDtoValidator : AbstractValidator<MarkAttendanceRequestDto>
{
    public MarkAttendanceRequestDtoValidator()
    {
        RuleFor(x => x.SessionId)
            .GreaterThan(0).WithMessage("SessionId must be greater than 0.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Attendance items must not be empty.");

        RuleForEach(x => x.Items)
            .SetValidator(new AttendanceItemDtoValidator());

        RuleFor(x => x.Items)
            .Must(items => items.Select(i => i.StudentId).Distinct().Count() == items.Count)
            .WithMessage("Duplicate StudentId in attendance items is not allowed.");
    }
}