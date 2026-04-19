using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.Invoices.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Invoices.Validators;

public class UpdateInvoiceRequestDtoValidator : AbstractValidator<UpdateInvoiceRequestDto>
{
    public UpdateInvoiceRequestDtoValidator()
    {
        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.");
    }
}
