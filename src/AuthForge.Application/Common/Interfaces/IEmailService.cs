using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(Email to, string userName, string verificationToken,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetAsync(Email to, string userName, string resetToken,
        CancellationToken cancellationToken = default);

    Task SendWelcomeEmailAsync(Email to, string userName, CancellationToken cancellationToken = default);
}