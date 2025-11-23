using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;
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

    public async Task InvokeAsync(
        HttpContext context,
        IConfigurationService configService,
        IEncryptionService encryptionService,
        AppDbContext dbContext)
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
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(token))
            {
                _logger.LogWarning("Malformed token received. First 50 chars: {TokenPreview}",
                    token.Substring(0, Math.Min(50, token.Length)));
                await _next(context);
                return;
            }

            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "user_type")?.Value;

            string secret;

            if (userTypeClaim == "admin")
            {
                var config = await configService.GetAsync();

                if (config?.JwtSecretEncrypted == null)
                {
                    _logger.LogWarning("Admin JWT secret not configured");
                    await _next(context);
                    return;
                }

                secret = encryptionService.Decrypt(config.JwtSecretEncrypted);
                _logger.LogDebug("Validating admin token with admin JWT secret");
            }
            else if (userTypeClaim == "user")
            {
                var applicationIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "application_id")?.Value;

                if (string.IsNullOrEmpty(applicationIdClaim) || !Guid.TryParse(applicationIdClaim, out var appId))
                {
                    _logger.LogWarning("User token missing valid application_id claim");
                    await _next(context);
                    return;
                }

                var application = await dbContext.Applications
                    .Where(a => a.Id == appId && !a.IsDeleted && a.IsActive)
                    .FirstOrDefaultAsync();

                if (application == null)
                {
                    _logger.LogWarning("Application {AppId} not found, deleted, or inactive", appId);
                    await _next(context);
                    return;
                }

                secret = encryptionService.Decrypt(application.JwtSecretEncrypted);
                _logger.LogDebug("Validating user token with application {AppId} JWT secret", appId);
            }
            else
            {
                _logger.LogWarning("Unknown user_type claim in token: {UserType}", userTypeClaim ?? "null");
                await _next(context);
                return;
            }

            var key = Encoding.UTF8.GetBytes(secret);

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

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Token validated for {UserType}: {UserId}", userTypeClaim, userId);
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