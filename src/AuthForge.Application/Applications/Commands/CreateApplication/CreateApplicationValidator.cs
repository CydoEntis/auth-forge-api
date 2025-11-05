// Application/Applications/Commands/CreateApplication/CreateApplicationCommandValidator.cs

using FluentValidation;

namespace AuthForge.Application.Applications.Commands.CreateApplication;

public sealed class CreateApplicationCommandValidator : AbstractValidator<CreateApplicationCommand>
{
    public CreateApplicationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

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
                .Must(o => !o.GoogleEnabled || (!string.IsNullOrEmpty(o.GoogleClientId) &&
                                                !string.IsNullOrEmpty(o.GoogleClientSecret)))
                .WithMessage("Google Client ID and Secret are required when Google is enabled");

            RuleFor(x => x.OAuthSettings!)
                .Must(o => !o.GithubEnabled || (!string.IsNullOrEmpty(o.GithubClientId) &&
                                                !string.IsNullOrEmpty(o.GithubClientSecret)))
                .WithMessage("Github Client ID and Secret are required when Github is enabled");
        });
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}