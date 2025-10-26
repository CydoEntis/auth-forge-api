namespace AuthForge.Application.Common.Interfaces;

public interface ISystemEmailService
{
    bool IsConfigured();
    
    Task SendAdminPasswordResetEmailAsync(
        string toEmail,
        string resetToken,
        CancellationToken cancellationToken = default);
}