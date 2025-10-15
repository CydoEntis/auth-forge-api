using FluentValidation;

namespace AuthForge.Application.Applications.Commands.Update;

public sealed class UpdateApplicationCommandValidator : AbstractValidator<UpdateApplicationCommand>
{
    public UpdateApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .MinimumLength(3);

        RuleFor(x => x.MaxFailedLoginAttempts)
            .GreaterThan(0)
            .LessThanOrEqualTo(10);

        RuleFor(x => x.LockoutDurationMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(1440); // Max 24 hours

        RuleFor(x => x.AccessTokenExpirationMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(1440); // Max 24 hours

        RuleFor(x => x.RefreshTokenExpirationDays)
            .GreaterThan(0)
            .LessThanOrEqualTo(90); // Max 90 days
    }
}