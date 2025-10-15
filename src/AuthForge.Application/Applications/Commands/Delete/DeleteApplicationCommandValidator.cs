using FluentValidation;

namespace AuthForge.Application.Applications.Commands.Delete;

public sealed class DeleteApplicationCommandValidator : AbstractValidator<DeleteApplicationCommand>
{
    public DeleteApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}