using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnglishCenter.Application.Features.Invoices.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Invoices.Validators;

public class SelectClassForInvoiceRequestDtoValidator : AbstractValidator<SelectClassForInvoiceRequestDto>
{
    public SelectClassForInvoiceRequestDtoValidator()
    {
        RuleFor(x => x.ClassId)
            .GreaterThan(0).WithMessage("ClassId must be greater than 0.");
    }
}
