using FluentValidation;

namespace AuthForge.Application.EndUsers.Commands.Refresh;

public sealed class RefreshEndUserTokenCommandValidator : AbstractValidator<RefreshEndUserTokenCommand>
{
    public RefreshEndUserTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}