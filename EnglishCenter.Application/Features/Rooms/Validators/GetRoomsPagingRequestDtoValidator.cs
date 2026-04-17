using EnglishCenter.Application.Features.Rooms.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Rooms.Validators;

public class GetRoomsPagingRequestDtoValidator : AbstractValidator<GetRoomsPagingRequestDto>
{
    public GetRoomsPagingRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100.");

        RuleFor(x => x.Keyword)
            .MaximumLength(100).WithMessage("Keyword must not exceed 100 characters.");

        RuleFor(x => x.Status)
            .Must(x => x == null || x == 0 || x == 1)
            .WithMessage("Status must be 0 or 1.");
    }
}
