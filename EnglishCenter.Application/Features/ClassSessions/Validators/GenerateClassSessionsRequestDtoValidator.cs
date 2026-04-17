using EnglishCenter.Application.Features.ClassSessions.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.ClassSessions.Validators;

public class GenerateClassSessionsRequestDtoValidator : AbstractValidator<GenerateClassSessionsRequestDto>
{
    public GenerateClassSessionsRequestDtoValidator()
    {
        RuleFor(x => x.ClassId)
            .GreaterThan(0).WithMessage("ClassId must be greater than 0.");
    }
}