using EnglishCenter.Application.Features.Rooms.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Rooms.Validators;

public class CreateRoomRequestDtoValidator : AbstractValidator<CreateRoomRequestDto>
{
    public CreateRoomRequestDtoValidator()
    {
        RuleFor(x => x.CampusId)
            .GreaterThan(0).WithMessage("CampusId must be greater than 0.");

        RuleFor(x => x.RoomCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("RoomCode is required.")
            .MaximumLength(50).WithMessage("RoomCode must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters.");

        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(0).WithMessage("Capacity must be greater than or equal to 0.");

        RuleFor(x => x.Status)
            .InclusiveBetween(0,1).WithMessage("Status must be 0 or 1.");
    }
}
