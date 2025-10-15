using FluentValidation;

namespace AuthForge.Application.EndUsers.Commands.Login;

public sealed class LoginEndUserCommandValidator : AbstractValidator<LoginEndUserCommand>
{
    public LoginEndUserCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}