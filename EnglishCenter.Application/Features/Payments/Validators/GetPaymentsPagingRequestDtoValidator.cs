using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.Payments.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Payments.Validators;

public class GetPaymentsPagingRequestDtoValidator : AbstractValidator<GetPaymentsPagingRequestDto>
{
    private static readonly string[] AllowedSortBy =
    [
        "Id", "InvoiceId", "Amount", "PaymentMethod", "PaymentDate", "CreatedAt"
    ];

    public GetPaymentsPagingRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100.");

        RuleFor(x => x.InvoiceId)
            .GreaterThan(0)
            .When(x => x.InvoiceId.HasValue)
            .WithMessage("InvoiceId must be greater than 0.");

        RuleFor(x => x.PaymentMethod)
            .Must(x => x == null || x == 1 || x == 2 || x == 3)
            .WithMessage("PaymentMethod is invalid.");

        RuleFor(x => x.TransactionCode)
            .MaximumLength(100).WithMessage("TransactionCode must not exceed 100 characters.");

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortBy.Contains(x))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortBy)}");

        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Equals("asc", StringComparison.OrdinalIgnoreCase) || x.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");
    }
}
