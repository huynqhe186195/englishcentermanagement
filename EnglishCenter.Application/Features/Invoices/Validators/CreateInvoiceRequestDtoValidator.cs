using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.Invoices.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Invoices.Validators;

public class CreateInvoiceRequestDtoValidator : AbstractValidator<CreateInvoiceRequestDto>
{
    public CreateInvoiceRequestDtoValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("StudentId must be greater than 0.");

        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("CourseId must be greater than 0.");

        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.");
    }
}
