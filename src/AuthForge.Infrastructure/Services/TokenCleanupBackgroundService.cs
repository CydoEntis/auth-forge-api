using AuthForge.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AuthForge.Infrastructure.Services;

public sealed class TokenCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TokenCleanupBackgroundService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

    public TokenCleanupBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<TokenCleanupBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Token Cleanup Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredTokensAsync(stoppingToken);
                
                _logger.LogDebug(
                    "Token cleanup completed. Next cleanup in {Hours} hours",
                    _cleanupInterval.TotalHours);

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Token Cleanup Background Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred during token cleanup. Will retry in {Hours} hours",
                    _cleanupInterval.TotalHours);

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }

    private async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var setupService = scope.ServiceProvider.GetRequiredService<ISetupService>();
        var isSetupComplete = await setupService.IsSetupCompleteAsync();

        if (!isSetupComplete)
        {
            _logger.LogDebug("Setup not complete. Skipping token cleanup");
            return;
        }

        var adminRefreshTokenRepo = scope.ServiceProvider
            .GetRequiredService<IAdminRefreshTokenRepository>();
        var endUserRefreshTokenRepo = scope.ServiceProvider
            .GetRequiredService<IEndUserRefreshTokenRepository>();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        _logger.LogInformation("Starting cleanup of expired refresh tokens");

        var expiredAdminTokens = await adminRefreshTokenRepo
            .GetExpiredTokensAsync(cancellationToken);

        if (expiredAdminTokens.Any())
        {
            foreach (var token in expiredAdminTokens)
            {
                adminRefreshTokenRepo.Remove(token);
            }

            _logger.LogInformation(
                "Removed {Count} expired admin refresh tokens",
                expiredAdminTokens.Count());
        }

        var expiredEndUserTokens = await endUserRefreshTokenRepo
            .GetExpiredTokensAsync(cancellationToken);

        if (expiredEndUserTokens.Any())
        {
            foreach (var token in expiredEndUserTokens)
            {
                endUserRefreshTokenRepo.Remove(token);
            }

            _logger.LogInformation(
                "Removed {Count} expired end user refresh tokens",
                expiredEndUserTokens.Count());
        }

        if (expiredAdminTokens.Any() || expiredEndUserTokens.Any())
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation(
                "Token cleanup completed successfully. Total removed: {Total}",
                expiredAdminTokens.Count() + expiredEndUserTokens.Count());
        }
        else
        {
            _logger.LogDebug("No expired tokens found to clean up");
        }
    }
}