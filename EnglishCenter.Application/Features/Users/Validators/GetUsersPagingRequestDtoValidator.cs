using EnglishCenter.Application.Features.Users.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Users.Validators;

public class GetUsersPagingRequestDtoValidator : AbstractValidator<GetUsersPagingRequestDto>
{
    public GetUsersPagingRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("PageNumber must be greater than 0.");
        RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("PageSize must be greater than 0.").LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100.");
        RuleFor(x => x.Keyword).MaximumLength(100).WithMessage("Keyword must not exceed 100 characters.");
    }
}
