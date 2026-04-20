using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnglishCenter.Application.Features.Auth.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Auth.Validators;

public class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestDtoValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("NewPassword is required.")
            .MinimumLength(6).WithMessage("NewPassword must be at least 6 characters.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("ConfirmPassword does not match NewPassword.");
    }
}   

