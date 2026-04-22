using EnglishCenter.Application.Features.Campus.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Campus.Validators;

public class CreateCampusRequestDtoValidator : AbstractValidator<CreateCampusRequestDto>
{
    public CreateCampusRequestDtoValidator()
    {
        RuleFor(x => x.CampusCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("CampusCode is required.")
            .MinimumLength(2).WithMessage("CampusCode must be at least 2 characters.")
            .MaximumLength(50).WithMessage("CampusCode must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.")
            .Matches(@"^[0-9+\-\s()]*$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Phone number format is invalid.");

        RuleFor(x => x.Status)
            .InclusiveBetween(0, 1).WithMessage("Status must be 0 or 1.");

        RuleFor(x => x.ManagerAdminUserId)
            .GreaterThan(0)
            .When(x => x.ManagerAdminUserId.HasValue)
            .WithMessage("ManagerAdminUserId must be greater than 0.");
    }
}
