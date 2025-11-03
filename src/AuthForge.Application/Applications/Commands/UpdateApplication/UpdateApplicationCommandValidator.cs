using FluentValidation;

namespace AuthForge.Application.Applications.Commands.UpdateApplication;

public sealed class UpdateApplicationCommandValidator : AbstractValidator<UpdateApplicationCommand>
{
    public UpdateApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required")
            .Must(BeValidGuid).WithMessage("Application ID must be a valid GUID");

        RuleFor(x => x.Name)
            .MinimumLength(3).WithMessage("Name must be at least 3 characters")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleForEach(x => x.AllowedOrigins)
            .Must(BeValidUrl).WithMessage("Each origin must be a valid URL")
            .When(x => x.AllowedOrigins != null && x.AllowedOrigins.Any());

        RuleFor(x => x.AllowedOrigins)
            .Must(x => x == null || x.Count <= 50).WithMessage("Maximum 50 allowed origins");

        When(x => x.EmailSettings != null, () =>
        {
            RuleFor(x => x.EmailSettings!.ApiKey)
                .NotEmpty().WithMessage("Email API key is required")
                .MinimumLength(10).WithMessage("API key must be at least 10 characters");

            RuleFor(x => x.EmailSettings!.FromEmail)
                .NotEmpty().WithMessage("From email is required")
                .EmailAddress().WithMessage("From email must be valid");

            RuleFor(x => x.EmailSettings!.FromName)
                .NotEmpty().WithMessage("From name is required")
                .MinimumLength(2).WithMessage("From name must be at least 2 characters");
        });

        When(x => x.OAuthSettings != null, () =>
        {
            RuleFor(x => x.OAuthSettings!)
                .Must(o => !o.GoogleEnabled ||
                           (!string.IsNullOrEmpty(o.GoogleClientId) &&
                            !string.IsNullOrEmpty(o.GoogleClientSecret)))
                .WithMessage("Google Client ID and Secret required when Google is enabled");

            RuleFor(x => x.OAuthSettings!)
                .Must(o => !o.GithubEnabled ||
                           (!string.IsNullOrEmpty(o.GithubClientId) &&
                            !string.IsNullOrEmpty(o.GithubClientSecret)))
                .WithMessage("Github Client ID and Secret required when Github is enabled");
        });
    }

    private bool BeValidGuid(string guid)
    {
        return Guid.TryParse(guid, out _);
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}