using AuthForge.Application.Common.Interfaces;
using AuthForge.Infrastructure.Data;
using AuthForge.Infrastructure.EmailProviders;
using AuthForge.Infrastructure.Extensions;
using AuthForge.Infrastructure.Repositories;
using AuthForge.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        // Setup service 
        services.AddScoped<ISetupService, SetupService>();

        return services;
    }
}