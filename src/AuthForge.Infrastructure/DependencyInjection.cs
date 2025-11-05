using AuthForge.Application.Common.Interfaces;
using AuthForge.Infrastructure.Data;
using AuthForge.Infrastructure.EmailProviders;
using AuthForge.Infrastructure.Extensions;
using AuthForge.Infrastructure.Repositories;
using AuthForge.Infrastructure.Security;
using AuthForge.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database context with dynamic configuration
        services.AddDatabaseConfiguration(configuration);

        // JWT authentication with dynamic configuration
        services.AddJwtAuthentication(configuration);

        // Repositories
        services.AddRepositories();
        services.AddApplicationServices();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Email services
        services.AddHttpClient();
        services.AddScoped<IEmailServiceFactory, EmailServiceFactory>();
        services.AddHttpClient<ISystemEmailService, SystemEmailService>();

        // Current user service
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Background services
        services.AddHostedService<TokenCleanupBackgroundService>();
        services.AddHostedService<EmailTokenCleanupBackgroundService>();

        // Setup services
        services.AddScoped<ISetupService, SetupService>();

        // Encryption services
        services.AddSingleton<IEncryptionService>(sp =>
        {
            var configDb = sp.GetRequiredService<ConfigurationDatabase>();
            var logger = sp.GetRequiredService<ILogger<AesEncryptionService>>();

            var isSetupComplete = configDb.GetBoolAsync("setup_complete").GetAwaiter().GetResult();

            if (!isSetupComplete)
            {
                return new NoOpEncryptionService();
            }

            return new AesEncryptionService(configDb, logger);
        });

        return services;
    }
}