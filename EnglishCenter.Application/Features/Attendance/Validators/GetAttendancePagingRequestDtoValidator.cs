using EnglishCenter.Application.Features.Attendance.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Attendance.Validators;

public class GetAttendancePagingRequestDtoValidator : AbstractValidator<GetAttendancePagingRequestDto>
{
    private static readonly string[] AllowedSortBy =
    [
        "Id", "SessionId", "StudentId", "Status", "CheckedAt"
    ];

    public GetAttendancePagingRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100.");

        RuleFor(x => x.SessionId)
            .GreaterThan(0)
            .When(x => x.SessionId.HasValue)
            .WithMessage("SessionId must be greater than 0.");

        RuleFor(x => x.StudentId)
            .GreaterThan(0)
            .When(x => x.StudentId.HasValue)
            .WithMessage("StudentId must be greater than 0.");

        RuleFor(x => x.Status)
            .Must(x => x == null || x == 1 || x == 2)
            .WithMessage("Status must be 1 or 2.");

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortBy.Contains(x))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortBy)}");

        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Equals("asc", StringComparison.OrdinalIgnoreCase) || x.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");
    }
}