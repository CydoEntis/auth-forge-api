using AuthForge.Api.Features.Applications.Shared.Models;
using FluentValidation;

namespace AuthForge.Api.Features.Applications.Shared.Validators;

public class OAuthSettingsRequestValidator : AbstractValidator<OAuthSettingsRequest>
{
    public OAuthSettingsRequestValidator()
    {
        RuleFor(x => x.Google)
            .SetValidator(new OAuthConfigValidator());

        RuleFor(x => x.Github)
            .SetValidator(new OAuthConfigValidator());
    }
}