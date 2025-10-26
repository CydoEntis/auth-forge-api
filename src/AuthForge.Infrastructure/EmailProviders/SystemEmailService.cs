using AuthForge.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AuthForge.Infrastructure.EmailProviders;

public sealed class SystemEmailService : ISystemEmailService
{
    private readonly ResendEmailService? _resendService;
    private readonly string _adminResetCallbackUrl;

    public SystemEmailService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        var apiKey = configuration["AuthForge:SystemEmail:ApiKey"];
        var fromEmail = configuration["AuthForge:SystemEmail:FromEmail"];
        var fromName = configuration["AuthForge:SystemEmail:FromName"] ?? "AuthForge";
        _adminResetCallbackUrl = configuration["AuthForge:SystemEmail:AdminResetCallbackUrl"] 
            ?? "http://localhost:5000/admin/reset-password";

        if (!string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(fromEmail))
        {
            _resendService = new ResendEmailService(
                httpClient,
                apiKey,
                fromEmail,
                fromName,
                passwordResetCallbackUrl: _adminResetCallbackUrl,
                emailVerificationCallbackUrl: null); 
        }
    }

    public bool IsConfigured() => _resendService is not null;

    public async Task SendAdminPasswordResetEmailAsync(
        string toEmail,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        if (_resendService is null)
        {
            throw new InvalidOperationException(
                "System email is not configured. Set AuthForge:SystemEmail settings in appsettings.json");
        }

        await _resendService.SendPasswordResetEmailAsync(
            toEmail: toEmail,
            toName: "Admin", 
            resetToken: resetToken,
            appName: "AuthForge Admin", 
            cancellationToken: cancellationToken);
    }
}