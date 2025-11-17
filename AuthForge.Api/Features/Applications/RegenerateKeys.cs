using FluentValidation;

namespace AuthForge.Api.Features.Applications;

public record RegenerateApplicationKeysRequest(ApplicationId ApplicationId);

public record RegenerateApplicationKeysResponse(
    string PublicKey,
    string SecretKey,
    DateTime RegeneratedAt,
    string Warning);

public class RegenerateApplicationKeysCommandValidator : AbstractValidator<RegenerateApplicationKeysRequest>
{
    public RegenerateApplicationKeysCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required");
    }
}

public class RegenerateKeys
{
    
}