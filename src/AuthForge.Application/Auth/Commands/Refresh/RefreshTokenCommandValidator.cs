using FluentValidation;

namespace AuthForge.Application.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.")
            .MinimumLength(20)
            .WithMessage("Invalid refresh token format.");
    }
}