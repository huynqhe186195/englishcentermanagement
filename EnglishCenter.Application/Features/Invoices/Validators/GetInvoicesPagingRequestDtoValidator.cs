using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.Invoices.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Invoices.Validators;

public class GetInvoicesPagingRequestDtoValidator : AbstractValidator<GetInvoicesPagingRequestDto>
{
    private static readonly string[] AllowedSortBy =
    [
        "Id", "InvoiceNo", "StudentId", "ClassId", "FinalAmount", "PaidAmount", "DueDate", "Status", "CreatedAt"
    ];

    public GetInvoicesPagingRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100.");

        RuleFor(x => x.InvoiceNo)
            .MaximumLength(50).WithMessage("InvoiceNo must not exceed 50 characters.");

        RuleFor(x => x.StudentId)
            .GreaterThan(0).When(x => x.StudentId.HasValue)
            .WithMessage("StudentId must be greater than 0.");

        RuleFor(x => x.ClassId)
            .GreaterThan(0).When(x => x.ClassId.HasValue)
            .WithMessage("ClassId must be greater than 0.");

        RuleFor(x => x.Status)
            .Must(x => x == null || x == 1 || x == 2 || x == 3 || x == 4)
            .WithMessage("Status is invalid.");

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortBy.Contains(x))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortBy)}");

        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Equals("asc", StringComparison.OrdinalIgnoreCase) || x.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");
    }
}
