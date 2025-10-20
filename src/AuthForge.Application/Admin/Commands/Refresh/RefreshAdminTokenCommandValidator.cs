using FluentValidation;

namespace AuthForge.Application.Admin.Commands.Refresh;

public class AdminRefreshTokenCommandValidator : AbstractValidator<RefreshAdminTokenCommand>
{
    public AdminRefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required")
            .MinimumLength(32).WithMessage("Invalid refresh token format");
    }
}