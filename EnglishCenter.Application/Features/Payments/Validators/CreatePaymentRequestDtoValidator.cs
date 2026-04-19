using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.Payments.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Payments.Validators;

public class CreatePaymentRequestDtoValidator : AbstractValidator<CreatePaymentRequestDto>
{
    public CreatePaymentRequestDtoValidator()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0).WithMessage("InvoiceId must be greater than 0.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.PaymentMethod)
            .Must(x => x == 1 || x == 2 || x == 3)
            .WithMessage("PaymentMethod is invalid.");

        RuleFor(x => x.TransactionCode)
            .MaximumLength(100).WithMessage("TransactionCode must not exceed 100 characters.");

        RuleFor(x => x.ReceivedByUserId)
            .GreaterThan(0)
            .When(x => x.ReceivedByUserId.HasValue)
            .WithMessage("ReceivedByUserId must be greater than 0.");

        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.");
    }
}
