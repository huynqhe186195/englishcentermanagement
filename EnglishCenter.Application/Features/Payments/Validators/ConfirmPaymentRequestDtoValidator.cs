using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.Payments.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Payments.Validators;

public class ConfirmPaymentRequestDtoValidator : AbstractValidator<ConfirmPaymentRequestDto>
{
    public ConfirmPaymentRequestDtoValidator()
    {
        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.");
    }
}
