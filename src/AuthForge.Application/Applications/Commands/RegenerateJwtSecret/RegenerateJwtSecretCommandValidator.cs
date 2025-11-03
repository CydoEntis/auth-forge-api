using FluentValidation;

namespace AuthForge.Application.Applications.Commands.RegenerateJwtSecret;

public class RegenerateJwtSecretCommandValidator : AbstractValidator<RegenerateJwtSecretCommand>
{
    public RegenerateJwtSecretCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required")
            .Must(BeValidGuid).WithMessage("Application ID must be a valid GUID");
    }

    private bool BeValidGuid(string guid)
    {
        return Guid.TryParse(guid, out _);
    }
}