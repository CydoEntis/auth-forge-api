using FluentValidation;

namespace AuthForge.Application.Applications.Commands.RegenerateApplicationKeys;

public class RegenerateApplicationKeysCommandValidator : AbstractValidator<RegenerateApplicationKeysCommand>
{
    public RegenerateApplicationKeysCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required");
    }
}