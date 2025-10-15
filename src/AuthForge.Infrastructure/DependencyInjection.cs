using AuthForge.Application.Common.Interfaces;
using AuthForge.Infrastructure.Authentication;
using AuthForge.Infrastructure.Data;
using AuthForge.Infrastructure.Repositories;
using AuthForge.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuthForgeDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(AuthForgeDbContext).Assembly.FullName)));

        services.AddScoped<IAuthForgeUserRepository, AuthForgeUserRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IEndUserRepository, EndUserRepository>();
        services.AddScoped<IAuthForgeRefreshTokenRepository, AuthForgeRefreshTokenRepository>();
        services.AddScoped<IEndUserRefreshTokenRepository, EndUserRefreshTokenRepository>();
        services.AddScoped<IAuthForgePasswordResetTokenRepository, AuthForgePasswordResetTokenRepository>();
        services.AddScoped<IEndUserPasswordResetTokenRepository, EndUserPasswordResetTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IAuthForgeJwtTokenGenerator, AuthForgeJwtTokenGenerator>();
        services.AddSingleton<IEndUserJwtTokenGenerator, EndUserJwtTokenGenerator>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}