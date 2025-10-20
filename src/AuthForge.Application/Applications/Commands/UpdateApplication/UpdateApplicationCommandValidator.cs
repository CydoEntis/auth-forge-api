using FluentValidation;

namespace AuthForge.Application.Applications.Commands.UpdateApplication;

public sealed class UpdateApplicationCommandValidator : AbstractValidator<UpdateApplicationCommand>
{
    public UpdateApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .MinimumLength(3);

        RuleFor(x => x.Settings)
            .NotNull();

        RuleFor(x => x.Settings.MaxFailedLoginAttempts)
            .GreaterThan(0)
            .LessThanOrEqualTo(10)
            .When(x => x.Settings != null);

        RuleFor(x => x.Settings.LockoutDurationMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(1440) // Max 24 hours
            .When(x => x.Settings != null);

        RuleFor(x => x.Settings.AccessTokenExpirationMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(1440) // Max 24 hours
            .When(x => x.Settings != null);

        RuleFor(x => x.Settings.RefreshTokenExpirationDays)
            .GreaterThan(0)
            .LessThanOrEqualTo(90) // Max 90 days
            .When(x => x.Settings != null);
    }
}