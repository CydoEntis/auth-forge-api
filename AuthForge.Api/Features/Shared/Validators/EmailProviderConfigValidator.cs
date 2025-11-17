using AuthForge.Api.Features.Shared.Enums;
using AuthForge.Api.Features.Shared.Models;
using FluentValidation;

namespace AuthForge.Api.Features.Shared.Validators;

public class EmailProviderConfigValidator : AbstractValidator<EmailProviderConfig>
{
    public EmailProviderConfigValidator()
    {
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
    }
}