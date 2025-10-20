using FluentValidation;

namespace AuthForge.Application.Applications.Commands.DeleteApplication;

public sealed class DeleteApplicationCommandValidator : AbstractValidator<DeleteApplicationCommand>
{
    public DeleteApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();
    }
}