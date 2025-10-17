using AuthForge.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthForge.Infrastructure.Extensions;

public static class JwtServiceExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthForgeSettings>(
            configuration.GetSection(AuthForgeSettings.SectionName));

        services.AddOptions<AuthForgeSettings>()
            .BindConfiguration(AuthForgeSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var authForgeSettings = configuration
            .GetSection(AuthForgeSettings.SectionName)
            .Get<AuthForgeSettings>();

        if (authForgeSettings?.Jwt == null)
        {
            throw new InvalidOperationException(
                "AuthForge:Jwt configuration is missing from appsettings.json");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = authForgeSettings.Jwt.Issuer,
                    ValidAudience = authForgeSettings.Jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(authForgeSettings.Jwt.Secret))
                };
            });

        services.AddAuthorization();

        Console.WriteLine("JWT Authentication configured successfully");

        return services;
    }
}