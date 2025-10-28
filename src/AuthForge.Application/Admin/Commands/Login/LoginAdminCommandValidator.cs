using FluentValidation;

namespace AuthForge.Application.Admin.Commands.Login;

public class AdminLoginCommandValidator : AbstractValidator<LoginAdminCommand>
{
    public AdminLoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}