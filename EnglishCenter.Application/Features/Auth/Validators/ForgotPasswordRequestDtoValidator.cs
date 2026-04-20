using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.Auth.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Auth.Validators;

public class ForgotPasswordRequestDtoValidator : AbstractValidator<ForgotPasswordRequestDto>
{
    public ForgotPasswordRequestDtoValidator()
    {
        RuleFor(x => x.EmailOrUserName)
            .NotEmpty().WithMessage("EmailOrUserName is required.")
            .MaximumLength(255).WithMessage("EmailOrUserName must not exceed 255 characters.");
    }
}
