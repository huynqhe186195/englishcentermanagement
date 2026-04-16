using FluentValidation;
using EnglishCenter.Application.Features.Roles.Dtos;

namespace EnglishCenter.Application.Features.Roles.Validators;

public class GetRolesPagingRequestDtoValidator : AbstractValidator<GetRolesPagingRequestDto>
{
    public GetRolesPagingRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("PageNumber must be greater than 0.");
        RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("PageSize must be greater than 0.").LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100.");
        RuleFor(x => x.Keyword).MaximumLength(100).WithMessage("Keyword must not exceed 100 characters.");
    }
}
