using EnglishCenter.Application.Features.Overrides.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Overrides.Validators;

public class ExecuteOverrideRequestDtoValidator : AbstractValidator<ExecuteOverrideRequestDto>
{
    private static readonly string[] SupportedActions =
    {
        "INVOICE_CANCEL",
        "ENROLLMENT_SUSPEND",
        "CLASSSESSION_CANCEL"
    };

    public ExecuteOverrideRequestDtoValidator()
    {
        RuleFor(x => x.ActionCode)
            .NotEmpty().WithMessage("ActionCode is required.")
            .Must(x => SupportedActions.Contains(x.Trim().ToUpperInvariant()))
            .WithMessage("Unsupported override action.");

        RuleFor(x => x.TargetId)
            .GreaterThan(0).WithMessage("TargetId must be greater than 0.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters.");

        RuleFor(x => x.Note)
            .MaximumLength(2000).WithMessage("Note must not exceed 2000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Note));
    }
}
