using AuthForge.Application.Common.Interfaces;
using AuthForge.Infrastructure.EmailProviders;
using AuthForge.Infrastructure.Extensions;
using AuthForge.Infrastructure.Repositories;
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

        return services;
    }
}