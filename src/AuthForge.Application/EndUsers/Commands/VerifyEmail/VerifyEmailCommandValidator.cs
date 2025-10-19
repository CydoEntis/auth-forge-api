using FluentValidation;

namespace AuthForge.Application.EndUsers.Commands.VerifyEmail;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required");

        RuleFor(x => x.VerificationToken)
            .NotEmpty().WithMessage("Verification token is required")
            .MinimumLength(10).WithMessage("Invalid verification token format");
    }
}