using AuthForge.Api.Features.Applications.Shared.Validators;
using FluentValidation;

namespace AuthForge.Api.Features.Applications;

public record RegenerateJwtSecretRequest(string ApplicationId);

public record RegenerateJwtSecretResponse(
    string JwtSecret,
    DateTime RegeneratedAt,
    string Warning);

public class RegenerateJwtSecretCommandValidator : AbstractValidator<RegenerateJwtSecretRequest>
{
    public RegenerateJwtSecretCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required")
            .MustBeValidGuid();
    }
}

public class RegenerateJwtSecret
{
}