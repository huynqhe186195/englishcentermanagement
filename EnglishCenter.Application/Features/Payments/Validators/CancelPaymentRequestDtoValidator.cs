using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.Payments.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Payments.Validators;

public class CancelPaymentRequestDtoValidator : AbstractValidator<CancelPaymentRequestDto>
{
    public CancelPaymentRequestDtoValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters.");
    }
}
