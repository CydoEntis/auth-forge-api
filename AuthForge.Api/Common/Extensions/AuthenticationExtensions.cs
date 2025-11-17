using System.Text;
using AuthForge.Api.Common.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AuthForge.Api.Common.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthForgeAuthentication(
        this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                    context.Token = token;
                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var configService = context.HttpContext.RequestServices.GetRequiredService<IConfigurationService>();
                    var config = await configService.GetAsync();
                    
                    if (config?.JwtSecretEncrypted == null)
                    {
                        context.Fail("JWT secret not configured");
                        return;
                    }

                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "AuthForge",
                        ValidAudience = "AuthForge",
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(config.JwtSecretEncrypted)),
                        ClockSkew = TimeSpan.Zero 
                    };

                }
            };

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "AuthForge",
                ValidAudience = "AuthForge",
                IssuerSigningKey = null,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => 
                policy.RequireRole("admin"));
        });

        return services;
    }
}