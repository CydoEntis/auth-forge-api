namespace AuthForge.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(
        string toEmail,
        string toName,
        string resetToken,
        string appName,
        CancellationToken cancellationToken = default);

    Task SendEmailVerificationEmailAsync(
        string toEmail,
        string toName,
        string verificationToken,
        string appName,
        CancellationToken cancellationToken = default);
}