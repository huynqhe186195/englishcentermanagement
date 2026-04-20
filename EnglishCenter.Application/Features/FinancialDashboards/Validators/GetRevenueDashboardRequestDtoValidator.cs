using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnglishCenter.Application.Features.FinancialDashboards.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.FinancialDashboards.Validators;

public class GetRevenueDashboardRequestDtoValidator : AbstractValidator<GetRevenueDashboardRequestDto>
{
    public GetRevenueDashboardRequestDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate <= x.ToDate)
            .WithMessage("FromDate must be less than or equal to ToDate.");
    }
}
