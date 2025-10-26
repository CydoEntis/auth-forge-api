using FluentValidation;

namespace AuthForge.Application.Admin.Commands.RequestPasswordReset;

public class RequestAdminPasswordResetCommandValidator
    : AbstractValidator<RequestAdminPasswordResetCommand>
{
    public RequestAdminPasswordResetCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}