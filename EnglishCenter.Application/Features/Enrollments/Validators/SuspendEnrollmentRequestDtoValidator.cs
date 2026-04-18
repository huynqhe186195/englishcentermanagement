using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.Enrollments.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Enrollments.Validators;

public class SuspendEnrollmentRequestDtoValidator : AbstractValidator<SuspendEnrollmentRequestDto>
{
    public SuspendEnrollmentRequestDtoValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
