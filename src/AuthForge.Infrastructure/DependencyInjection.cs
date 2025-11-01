using AuthForge.Application.Common.Interfaces;
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
        services.AddDatabaseConfiguration(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddRepositories();
        services.AddApplicationServices();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        
        services.AddHttpClient();
        services.AddScoped<IEmailServiceFactory, EmailServiceFactory>();
        services.AddHttpClient<ISystemEmailService, SystemEmailService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHostedService<TokenCleanupBackgroundService>();
        services.AddHostedService<EmailTokenCleanupBackgroundService>();
        
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<ISetupService, SetupService>();
        return services;
    }
}