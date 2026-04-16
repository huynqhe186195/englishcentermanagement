using EnglishCenter.Application.Features.Courses.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Courses.Validators;

public class GetCoursesPagingRequestDtoValidator : AbstractValidator<GetCoursesPagingRequestDto>
{
    private static readonly string[] AllowedSortBy =
    [
        "Id", "CourseCode", "Name", "Level", "TotalSessions", "DefaultFee", "Status", "CreatedAt"
    ];

    public GetCoursesPagingRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100.");

        RuleFor(x => x.Keyword)
            .MaximumLength(100).WithMessage("Keyword must not exceed 100 characters.");

        RuleFor(x => x.Status)
            .Must(x => x == null || x == 0 || x == 1)
            .WithMessage("Status must be 0 or 1.");

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortBy.Contains(x))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortBy)}");

        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Equals("asc", StringComparison.OrdinalIgnoreCase) || x.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");
    }
}