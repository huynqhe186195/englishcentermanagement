using EnglishCenter.Application.Features.Notifications.Dtos;
using FluentValidation;

namespace EnglishCenter.Application.Features.Notifications.Validators;

public class CreateNotificationRequestDtoValidator : AbstractValidator<CreateNotificationRequestDto>
{
    public CreateNotificationRequestDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.").MaximumLength(255).WithMessage("Title must not exceed 255 characters.");
        RuleFor(x => x.Content).NotEmpty().WithMessage("Content is required.");
        RuleFor(x => x.Channel).InclusiveBetween(0, 10).WithMessage("Channel is invalid.");
    }
}
