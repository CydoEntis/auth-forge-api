using FluentValidation;

namespace AuthForge.Application.EndUsers.Commands.LockEndUser;

public class LockEndUserCommandValidator : AbstractValidator<LockEndUserCommand>
{
    public LockEndUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.LockoutMinutes)
            .GreaterThan(0).WithMessage("Lockout duration must be greater than 0")
            .LessThanOrEqualTo(43200).WithMessage("Lockout duration cannot exceed 30 days (43200 minutes)");
    }
}