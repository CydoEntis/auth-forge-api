using AuthForge.Api.Features.Applications.Shared.Models;
using AuthForge.Api.Features.Applications.Shared.Validators;
using AuthForge.Api.Features.Shared.Models;
using AuthForge.Api.Features.Shared.Validators;
using FluentValidation;

namespace AuthForge.Api.Features.Applications;

public sealed record CreateApplicationRequest(
    string Name,
    string? Description,
    List<string>? AllowedOrigins,
    EmailProviderConfig EmailConfig,
    OAuthSettingsRequest OAuthSettings);

public sealed record CreateApplicationResponse(
    string ApplicationId,
    string Name,
    string Slug,
    string? Description,
    ApplicationKeys Keys,
    string JwtSecret,
    List<string> AllowedOrigins,
    bool IsActive,
    DateTime CreatedAtUtc);

public class CreateApplicationRequestValidator : AbstractValidator<CreateApplicationRequest>
{
    public CreateApplicationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        When(x => x.AllowedOrigins != null && x.AllowedOrigins.Any(), () =>
        {
            RuleForEach(x => x.AllowedOrigins)
                .MustBeValidUrl();

            RuleFor(x => x.AllowedOrigins)
                .Must(x => x.Count <= 50)
                .WithMessage("Maximum 50 allowed origins");
        });

        RuleFor(x => x.EmailConfig)
            .SetValidator(new EmailProviderConfigValidator());

        RuleFor(x => x.OAuthSettings)
            .SetValidator(new OAuthSettingsRequestValidator());
    }
}

public class CreateApplication
{
}