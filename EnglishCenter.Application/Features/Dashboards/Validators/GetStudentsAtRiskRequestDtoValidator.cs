using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.Dashboards.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Dashboards.Validators;

public class GetStudentsAtRiskRequestDtoValidator : AbstractValidator<GetStudentsAtRiskRequestDto>
{
    public GetStudentsAtRiskRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100.");

        RuleFor(x => x.AttendanceThreshold)
            .InclusiveBetween(0, 100).WithMessage("AttendanceThreshold must be between 0 and 100.");
    }
}
