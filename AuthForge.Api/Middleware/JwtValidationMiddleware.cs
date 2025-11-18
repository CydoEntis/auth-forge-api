using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthForge.Api.Common.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace AuthForge.Api.Middleware;

public class JwtValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtValidationMiddleware> _logger;

    public JwtValidationMiddleware(RequestDelegate next, ILogger<JwtValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IConfigurationService configService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var config = await configService.GetAsync();

                if (config?.JwtSecretEncrypted == null)
                {
                    _logger.LogWarning("JWT secret not configured");
                    await _next(context);
                    return;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(config.JwtSecretEncrypted);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "AuthForge",
                    ValidAudience = "AuthForge",
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                context.User = principal;

                _logger.LogInformation("Token validated for admin: {AdminId}",
                    principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("Token expired");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed");
            }
        }

        await _next(context);
    }
}