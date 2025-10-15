using FluentValidation;

namespace AuthForge.Application.AuthForge.Commands.Login;

public sealed class LoginDeveloperCommandValidator : AbstractValidator<LoginDeveloperCommand>
{
    public LoginDeveloperCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}