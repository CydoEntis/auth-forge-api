using FluentValidation;

namespace AuthForge.Application.Applications.Commands.AllowedOrigins.UpdateAllowedOrigin;

public class UpdateAllowedOriginCommandValidator : AbstractValidator<UpdateAllowedOriginCommand>
{
    public UpdateAllowedOriginCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required");

        RuleFor(x => x.OldOrigin)
            .NotEmpty().WithMessage("Old origin is required")
            .Must(BeValidUrl).WithMessage("Old origin must be a valid URL");

        RuleFor(x => x.NewOrigin)
            .NotEmpty().WithMessage("New origin is required")
            .Must(BeValidUrl).WithMessage("New origin must be a valid URL");
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}