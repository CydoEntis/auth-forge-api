using FluentValidation;

namespace AuthForge.Application.EndUsers.Commands.DeactivateEndUser;

public class DeactivateEndUserCommandValidator : AbstractValidator<DeactivateEndUserCommand>
{
    public DeactivateEndUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}