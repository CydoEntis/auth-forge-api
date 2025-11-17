using AuthForge.Api.Features.Applications.Shared.Models;
using FluentValidation;

namespace AuthForge.Api.Features.Applications.Shared.Validators;

public class OAuthConfigValidator : AbstractValidator<OAuthConfig>
{
    public OAuthConfigValidator()
    {
        RuleFor(x => x.Enabled)
            .NotNull().WithMessage("Provider must be set to enabled or disabled");

        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("Provider ClientId is required when enabled");
            RuleFor(x => x.ClientSecret)
                .NotEmpty().WithMessage("Provider ClientSecret is required when enabled");
        });
    }
}