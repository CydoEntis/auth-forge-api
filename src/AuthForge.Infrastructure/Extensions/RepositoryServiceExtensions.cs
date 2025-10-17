using AuthForge.Application.Common.Interfaces;
using AuthForge.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AuthForge.Infrastructure.Extensions;

public static class RepositoryServiceExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAuthForgeUserRepository, AuthForgeUserRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IEndUserRepository, EndUserRepository>();
        services.AddScoped<IAuthForgeRefreshTokenRepository, AuthForgeRefreshTokenRepository>();
        services.AddScoped<IEndUserRefreshTokenRepository, EndUserRefreshTokenRepository>();
        services.AddScoped<IAuthForgePasswordResetTokenRepository, AuthForgePasswordResetTokenRepository>();
        services.AddScoped<IEndUserPasswordResetTokenRepository, EndUserPasswordResetTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}