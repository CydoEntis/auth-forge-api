using FluentValidation;

namespace AuthForge.Application.EndUsers.Commands.ActivateEndUser;

public class ActivateEndUserCommandValidator : AbstractValidator<ActivateEndUserCommand>
{
    public ActivateEndUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}