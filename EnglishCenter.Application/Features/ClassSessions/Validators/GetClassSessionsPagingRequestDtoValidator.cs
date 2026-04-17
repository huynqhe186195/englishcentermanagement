using EnglishCenter.Application.Features.ClassSessions.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.ClassSessions.Validators;

public class GetClassSessionsPagingRequestDtoValidator : AbstractValidator<GetClassSessionsPagingRequestDto>
{
    private static readonly string[] AllowedSortBy =
    [
        "Id", "SessionNo", "SessionDate", "StartTime", "EndTime", "Status", "CreatedAt"
    ];

    public GetClassSessionsPagingRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100.");

        RuleFor(x => x.ClassId)
            .GreaterThan(0)
            .When(x => x.ClassId.HasValue)
            .WithMessage("ClassId must be greater than 0.");

        RuleFor(x => x.Status)
            .Must(x => x == null || (x >= 1 && x <= 4))
            .WithMessage("Status must be between 1 and 4.");

        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate <= x.ToDate)
            .WithMessage("FromDate must be less than or equal to ToDate.");

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortBy.Contains(x))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortBy)}");

        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Equals("asc", StringComparison.OrdinalIgnoreCase) || x.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");
    }
}