using FluentValidation;

namespace AuthForge.Application.EndUsers.Commands.UnlockEndUser;

public class UnlockEndUserCommandValidator : AbstractValidator<UnlockEndUserCommand>
{
    public UnlockEndUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}