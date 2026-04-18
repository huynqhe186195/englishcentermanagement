using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.ClassSessions.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.ClassSessions.Validators;

public class CompleteClassSessionRequestDtoValidator : AbstractValidator<CompleteClassSessionRequestDto>
{
    public CompleteClassSessionRequestDtoValidator()
    {
        RuleFor(x => x.Note)
            .MaximumLength(1000)
            .WithMessage("Note must not exceed 1000 characters.");
    }
}
