using FluentValidation;
using AuthForge.Api.Features.Shared.Models;

namespace AuthForge.Api.Features.Shared.Validators;

public class TestEmailConfigValidator : AbstractValidator<TestEmailConfigRequest>
{
    public TestEmailConfigValidator()
    {
        RuleFor(x => x.FromEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.TestRecipient).NotEmpty().EmailAddress();

        When(x => !string.IsNullOrEmpty(x.SmtpHost), () =>
        {
            RuleFor(x => x.SmtpHost).NotEmpty();
            RuleFor(x => x.SmtpPort).GreaterThan(0).WithMessage("Port must be greater than 0");
            RuleFor(x => x.SmtpUsername).NotEmpty();
            RuleFor(x => x.SmtpPassword).NotEmpty();
        });

        When(x => !string.IsNullOrEmpty(x.ResendApiKey), () => { RuleFor(x => x.ResendApiKey).NotEmpty(); });

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.SmtpHost) || !string.IsNullOrEmpty(x.ResendApiKey))
            .WithMessage("Either SMTP or Resend configuration is required");
    }
}