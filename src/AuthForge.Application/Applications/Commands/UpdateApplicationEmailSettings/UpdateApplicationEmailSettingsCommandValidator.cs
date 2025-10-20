using FluentValidation;

namespace AuthForge.Application.Applications.Commands.UpdateApplicationEmailSettings;

public class UpdateApplicationEmailSettingsCommandValidator : AbstractValidator<UpdateApplicationEmailSettingsCommand>
{
    public UpdateApplicationEmailSettingsCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required");

        RuleFor(x => x.Provider)
            .IsInEnum().WithMessage("Invalid email provider");

        RuleFor(x => x.ApiKey)
            .NotEmpty().WithMessage("API key is required")
            .MinimumLength(10).WithMessage("API key must be at least 10 characters");

        RuleFor(x => x.FromEmail)
            .NotEmpty().WithMessage("From email is required")
            .EmailAddress().WithMessage("From email must be a valid email address");

        RuleFor(x => x.FromName)
            .NotEmpty().WithMessage("From name is required")
            .MinimumLength(2).WithMessage("From name must be at least 2 characters")
            .MaximumLength(100).WithMessage("From name must not exceed 100 characters");

        RuleFor(x => x.PasswordResetCallbackUrl)
            .Must(BeValidUrlOrNull).WithMessage("Password reset callback URL must be a valid URL")
            .When(x => !string.IsNullOrWhiteSpace(x.PasswordResetCallbackUrl));

        RuleFor(x => x.EmailVerificationCallbackUrl)
            .Must(BeValidUrlOrNull).WithMessage("Email verification callback URL must be a valid URL")
            .When(x => !string.IsNullOrWhiteSpace(x.EmailVerificationCallbackUrl));
    }

    private bool BeValidUrlOrNull(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}