using FluentValidation;

namespace AuthForge.Application.EndUsers.Commands.DeleteEndUser;

public class DeleteEndUserCommandValidator : AbstractValidator<DeleteEndUserCommand>
{
    public DeleteEndUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}