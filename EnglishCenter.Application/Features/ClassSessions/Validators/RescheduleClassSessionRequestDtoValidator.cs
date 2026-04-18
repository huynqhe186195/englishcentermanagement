using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.ClassSessions.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.ClassSessions.Validators;

public class RescheduleClassSessionRequestDtoValidator : AbstractValidator<RescheduleClassSessionRequestDto>
{
    public RescheduleClassSessionRequestDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => x.StartTime < x.EndTime)
            .WithMessage("StartTime must be less than EndTime.");

        RuleFor(x => x.RoomId)
            .GreaterThan(0)
            .When(x => x.RoomId.HasValue)
            .WithMessage("RoomId must be greater than 0.");

        RuleFor(x => x.TeacherId)
            .GreaterThan(0)
            .When(x => x.TeacherId.HasValue)
            .WithMessage("TeacherId must be greater than 0.");

        RuleFor(x => x.Note)
            .MaximumLength(1000)
            .WithMessage("Note must not exceed 1000 characters.");
    }
}
