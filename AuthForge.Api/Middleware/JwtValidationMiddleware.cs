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
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader))
        {
            await _next(context);
            return;
        }

        var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader.Substring("Bearer ".Length).Trim()
            : authHeader;

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Empty token after parsing Authorization header");
            await _next(context);
            return;
        }

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

            if (!tokenHandler.CanReadToken(token))
            {
                _logger.LogWarning("Malformed token received. First 50 chars: {TokenPreview}",
                    token.Substring(0, Math.Min(50, token.Length)));
                await _next(context);
                return;
            }

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
        catch (SecurityTokenMalformedException ex)
        {
            _logger.LogWarning(ex, "Malformed token: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed: {Message}", ex.Message);
        }

        await _next(context);
    }
}