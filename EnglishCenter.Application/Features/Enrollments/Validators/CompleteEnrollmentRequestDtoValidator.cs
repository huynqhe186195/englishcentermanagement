using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.Enrollments.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Enrollments.Validators;

public class CompleteEnrollmentRequestDtoValidator : AbstractValidator<CompleteEnrollmentRequestDto>
{
    public CompleteEnrollmentRequestDtoValidator()
    {
        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("Note must not exceed 500 characters.");
    }
}
