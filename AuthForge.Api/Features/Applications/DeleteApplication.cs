using FluentValidation;

namespace AuthForge.Api.Features.Applications;

public sealed record DeleteApplicationRequest(
    string ApplicationId);

public sealed class DeleteApplicationCommandValidator : AbstractValidator<DeleteApplicationRequest>
{
    public DeleteApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();
    }
}

public class DeleteApplication
{
}