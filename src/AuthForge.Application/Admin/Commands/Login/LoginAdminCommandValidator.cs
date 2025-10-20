using FluentValidation;

namespace AuthForge.Application.Admin.Commands.Login;

public class AdminLoginCommandValidator : AbstractValidator<LoginAdminCommand>
{
    public AdminLoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");
    }
}