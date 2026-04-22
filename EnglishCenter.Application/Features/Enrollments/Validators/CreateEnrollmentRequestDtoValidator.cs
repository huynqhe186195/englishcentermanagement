using EnglishCenter.Application.Features.Enrollments.Dtos;
using EnglishCenter.Application.Common.Interfaces;
using EnglishCenter.Domain.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;


namespace EnglishCenter.Application.Features.Enrollments.Validators;

public class CreateEnrollmentRequestDtoValidator : AbstractValidator<CreateEnrollmentRequestDto>
{
    public CreateEnrollmentRequestDtoValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0)
            .WithMessage("StudentId must be greater than 0.");

        RuleFor(x => x.ClassId)
            .GreaterThan(0)
            .WithMessage("ClassId must be greater than 0.");

        RuleFor(x => x.EnrollDate)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("EnrollDate cannot be in the future.");

        RuleFor(x => x.Note)
            .MaximumLength(1000)
            .WithMessage("Note must not exceed 1000 characters.");

        RuleFor(x => x.Status)
            .InclusiveBetween(0, 1)
            .WithMessage("Status must be 0 or 1.");
    }
}
