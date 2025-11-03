using AuthForge.Domain.Enums;
using FluentValidation;

namespace AuthForge.Application.Setup.Commands.CompleteSetup;

public class CompleteSetupCommandValidator : AbstractValidator<CompleteSetupCommand>
{
    public CompleteSetupCommandValidator()
    {
        RuleFor(x => x.DatabaseType)
            .IsInEnum()
            .WithMessage("Invalid database type");

        When(x => x.DatabaseType == DatabaseType.PostgreSql, () =>
        {
            RuleFor(x => x.ConnectionString)
                .NotEmpty()
                .WithMessage("Connection string is required for PostgreSQL")
                .WithErrorCode("Validation.ConnectionString");
        });

        RuleFor(x => x.EmailProvider)
            .IsInEnum()
            .WithMessage("Invalid email provider");

        RuleFor(x => x.FromEmail)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Valid from email is required")
            .WithErrorCode("Validation.FromEmail");

        RuleFor(x => x.FromName)
            .NotEmpty()
            .WithMessage("From name is required")
            .WithErrorCode("Validation.FromName");

        When(x => x.EmailProvider == EmailProvider.Resend, () =>
        {
            RuleFor(x => x.ResendApiKey)
                .NotEmpty()
                .WithMessage("Resend API key is required")
                .WithErrorCode("Validation.ResendApiKey");
        });

        When(x => x.EmailProvider == EmailProvider.Smtp, () =>
        {
            RuleFor(x => x.SmtpHost)
                .NotEmpty()
                .WithMessage("SMTP host is required")
                .WithErrorCode("Validation.SmtpHost");

            RuleFor(x => x.SmtpPort)
                .GreaterThan(0)
                .LessThanOrEqualTo(65535)
                .WithMessage("SMTP port must be between 1 and 65535")
                .WithErrorCode("Validation.SmtpPort");

            RuleFor(x => x.SmtpUsername)
                .NotEmpty()
                .WithMessage("SMTP username is required")
                .WithErrorCode("Validation.SmtpUsername");

            RuleFor(x => x.SmtpPassword)
                .NotEmpty()
                .WithMessage("SMTP password is required")
                .WithErrorCode("Validation.SmtpPassword");
        });

        RuleFor(x => x.AdminEmail)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Valid admin email is required")
            .WithErrorCode("Validation.AdminEmail");

        RuleFor(x => x.AdminPassword)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Admin password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character")
            .WithErrorCode("Validation.AdminPassword");
    }
}