using FluentValidation;

namespace AuthForge.Application.AuthForge.Commands.Refresh;

public sealed class RefreshDeveloperTokenCommandValidator : AbstractValidator<RefreshDeveloperTokenCommand>
{
    public RefreshDeveloperTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}