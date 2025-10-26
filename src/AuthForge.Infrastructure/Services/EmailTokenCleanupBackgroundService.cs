using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AuthForge.Infrastructure.Services;

public sealed class EmailTokenCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<EmailTokenCleanupBackgroundService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); 

    public EmailTokenCleanupBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<EmailTokenCleanupBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Token Cleanup Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredTokensAsync(stoppingToken);

                _logger.LogDebug(
                    "Email token cleanup completed. Next cleanup in {Hours} hours",
                    _cleanupInterval.TotalHours);

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Email Token Cleanup Background Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred during email token cleanup. Will retry in {Hours} hours",
                    _cleanupInterval.TotalHours);

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }

    private async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AuthForgeDbContext>();

        _logger.LogInformation("Starting cleanup of expired email and password reset tokens");

        var now = DateTime.UtcNow;

        var usersWithExpiredTokens = await context.EndUsers
            .Where(u => (u.EmailVerificationToken != null &&
                         u.EmailVerificationTokenExpiresAt != null &&
                         u.EmailVerificationTokenExpiresAt < now) ||
                        (u.PasswordResetToken != null &&
                         u.PasswordResetTokenExpiresAt != null &&
                         u.PasswordResetTokenExpiresAt < now))
            .ToListAsync(cancellationToken);

        var adminsWithExpiredTokens = await context.Admins
            .Where(a => a.PasswordResetToken != null &&
                        a.PasswordResetTokenExpiresAt != null &&
                        a.PasswordResetTokenExpiresAt < now)
            .ToListAsync(cancellationToken);

        int verificationTokensCleared = 0;
        int endUserResetTokensCleared = 0;

        // Clear expired tokens using entity methods
        foreach (var user in usersWithExpiredTokens)
        {
            if (user.IsEmailVerificationTokenExpired())
            {
                user.ClearExpiredEmailVerificationToken();
                verificationTokensCleared++;
            }

            user.ClearExpiredPasswordResetToken();
            if (user.PasswordResetToken == null)
            {
                endUserResetTokensCleared++;
            }
        }

        int adminResetTokensCleared = 0;
        foreach (var admin in adminsWithExpiredTokens)
        {
            admin.ClearExpiredPasswordResetToken();
            adminResetTokensCleared++;
        }

        var totalCleaned = verificationTokensCleared + endUserResetTokensCleared + adminResetTokensCleared;

        if (totalCleaned > 0)
        {
            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Cleaned up {Total} expired tokens ({VerificationTokens} verification, {EndUserResetTokens} end-user reset, {AdminResetTokens} admin reset)",
                totalCleaned,
                verificationTokensCleared,
                endUserResetTokensCleared,
                adminResetTokensCleared);
        }
        else
        {
            _logger.LogDebug("No expired tokens found to clean up");
        }
    }
}