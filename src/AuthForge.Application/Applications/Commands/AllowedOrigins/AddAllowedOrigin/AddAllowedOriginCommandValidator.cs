using FluentValidation;

namespace AuthForge.Application.Applications.Commands.AllowedOrigins.AddAllowedOrigin;

public class AddAllowedOriginCommandValidator : AbstractValidator<AddAllowedOriginCommand>
{
    public AddAllowedOriginCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required");

        RuleFor(x => x.Origin)
            .NotEmpty().WithMessage("Origin is required")
            .Must(BeValidUrl).WithMessage("Origin must be a valid URL");
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}