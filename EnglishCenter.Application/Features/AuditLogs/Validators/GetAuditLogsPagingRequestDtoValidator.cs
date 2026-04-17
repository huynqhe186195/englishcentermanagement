using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.AuditLogs.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.AuditLogs.Validators;

public class GetAuditLogsPagingRequestDtoValidator : AbstractValidator<GetAuditLogsPagingRequestDto>
{
    private static readonly string[] AllowedSortBy =
    [
        "Id", "UserId", "Action", "EntityName", "EntityId", "CreatedAt"
    ];

    public GetAuditLogsPagingRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100.");

        RuleFor(x => x.EntityName)
            .MaximumLength(255).WithMessage("EntityName must not exceed 255 characters.");

        RuleFor(x => x.Action)
            .MaximumLength(20).WithMessage("Action must not exceed 20 characters.");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .When(x => x.UserId.HasValue)
            .WithMessage("UserId must be greater than 0.");

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortBy.Contains(x))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortBy)}");

        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Equals("asc", StringComparison.OrdinalIgnoreCase) || x.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");
    }
}
