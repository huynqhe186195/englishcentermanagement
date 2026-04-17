using EnglishCenter.Application.Features.ClassSessions.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.ClassSessions.Validators;

public class UpdateClassSessionRequestDtoValidator : AbstractValidator<UpdateClassSessionRequestDto>
{
    public UpdateClassSessionRequestDtoValidator()
    {
        RuleFor(x => x.SessionNo)
            .GreaterThan(0).WithMessage("SessionNo must be greater than 0.");

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

        RuleFor(x => x.Topic)
            .MaximumLength(255).WithMessage("Topic must not exceed 255 characters.");

        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.");

        RuleFor(x => x.Status)
            .InclusiveBetween(1, 4).WithMessage("Status must be between 1 and 4.");
    }
}