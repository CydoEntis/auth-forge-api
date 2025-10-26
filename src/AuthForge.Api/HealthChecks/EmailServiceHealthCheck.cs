using AuthForge.Application.Common.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AuthForge.Api.HealthChecks;

public class EmailServiceHealthCheck : IHealthCheck
{
    private readonly ISystemEmailService _emailService;
    private readonly ILogger<EmailServiceHealthCheck> _logger;

    public EmailServiceHealthCheck(
        ISystemEmailService emailService,
        ILogger<EmailServiceHealthCheck> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_emailService.IsConfigured())
            {
                _logger.LogWarning("Email service health check: Not configured");
                
                return Task.FromResult(
                    HealthCheckResult.Degraded(
                        description: "Email service is not configured. Admin password resets will log to console."));
            }

            _logger.LogDebug("Email service health check: Healthy");
            
            return Task.FromResult(
                HealthCheckResult.Healthy(
                    description: "Email service is configured and ready."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email service health check failed");
            
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    description: "Email service check failed.",
                    exception: ex));
        }
    }
}