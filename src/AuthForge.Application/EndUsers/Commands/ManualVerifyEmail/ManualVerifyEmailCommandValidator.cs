using FluentValidation;

namespace AuthForge.Application.EndUsers.Commands.ManualVerifyEmail;

public class ManualVerifyEmailCommandValidator : AbstractValidator<ManualVerifyEmailCommand>
{
    public ManualVerifyEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}