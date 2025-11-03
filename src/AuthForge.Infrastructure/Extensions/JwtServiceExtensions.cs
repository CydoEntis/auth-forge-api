using System.Security.Claims;
using System.Text;
using AuthForge.Application.Common.Settings;
using AuthForge.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthForge.Infrastructure.Extensions;

public static class JwtServiceExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerDynamicConfiguration>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(); 

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c =>
                        (c.Type == "role" || c.Type == ClaimTypes.Role) &&
                        c.Value == "Admin")));
        });

        Console.WriteLine("JWT Authentication configured with dynamic resolution");

        return services;
    }
}

public class JwtBearerDynamicConfiguration : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly ConfigurationDatabase _configDb;

    public JwtBearerDynamicConfiguration(ConfigurationDatabase configDb)
    {
        _configDb = configDb;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme)
            return;

        var setupComplete = _configDb.GetBoolAsync("setup_complete").GetAwaiter().GetResult();

        if (!setupComplete)
        {
            ConfigurePlaceholder(options);
            return;
        }

        var settings = _configDb.GetAllAsync().GetAwaiter().GetResult();
        var jwtSecret = settings.GetValueOrDefault("jwt_secret");
        var jwtIssuer = settings.GetValueOrDefault("jwt_issuer", "AuthForge");
        var jwtAudience = settings.GetValueOrDefault("jwt_audience", "AuthForgeClient");

        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new InvalidOperationException("JWT secret not found in configuration database");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };

        Console.WriteLine("JWT configured from configuration database");
    }

    public void Configure(JwtBearerOptions options) => Configure(JwtBearerDefaults.AuthenticationScheme, options);

    private static void ConfigurePlaceholder(JwtBearerOptions options)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false
        };

        Console.WriteLine("JWT configured in setup mode (placeholder)");
    }
}