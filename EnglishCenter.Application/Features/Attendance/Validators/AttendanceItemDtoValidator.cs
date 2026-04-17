using EnglishCenter.Application.Features.Attendance.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Attendance.Validators;

public class AttendanceItemDtoValidator : AbstractValidator<AttendanceItemDto>
{
    public AttendanceItemDtoValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("StudentId must be greater than 0.");

        RuleFor(x => x.Status)
            .Must(x => x == 1 || x == 2)
            .WithMessage("Status must be 1 (Present) or 2 (Absent).");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("Note must not exceed 500 characters.");
    }
}