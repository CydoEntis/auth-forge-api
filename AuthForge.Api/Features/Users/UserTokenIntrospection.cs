using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthForge.Api.Common;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthForge.Api.Features.Users;

// ============================================================================
// TOKEN INTROSPECTION - THE MULTI-LANGUAGE FOUNDATION
// ============================================================================
// This endpoint solves the "How do I validate AuthForge tokens in PHP/Python/Go?"
// problem. Instead of sharing JWT secrets or requiring language-specific SDKs,
// applications just call this endpoint with a token and get back user info.
//
// Flow:
// 1. Client app receives user's access token
// 2. Client app calls: POST /api/v1/apps/{appId}/auth/introspect
// 3. AuthForge validates the token using the app's JWT secret
// 4. Returns user info if valid, error if not
//
// This is based on RFC 7662 (OAuth 2.0 Token Introspection)
// https://datatracker.ietf.org/doc/html/rfc7662
// ============================================================================

public sealed record IntrospectTokenRequest(
    string Token,
    string? TokenTypeHint = null // "access_token" or "refresh_token"
);

public sealed record IntrospectTokenResponse(
    bool Active,
    string? Sub, // user Id
    string? Email, // User's email
    string? ClientId, // Application client ID that issued the token
    long? Exp,
    long? Iat,
    string? Scope, // Scopes/permissions (future use)
    string? TokenType, // "access_token" or "refresh_token"
    Guid? ApplicationId,
    Dictionary<string, object>? AdditionalClaims = null
);

public sealed class IntrospectTokenValidator : AbstractValidator<IntrospectTokenRequest>
{
    public IntrospectTokenValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Token is required");

        When(x => !string.IsNullOrEmpty(x.TokenTypeHint), () =>
        {
            RuleFor(x => x.TokenTypeHint)
                .Must(hint => hint == "access_token" || hint == "refresh_token")
                .WithMessage("TokenTypeHint must be 'access_token' or 'refresh_token'");
        });
    }
}

public sealed class IntrospectTokenHandler
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<IntrospectTokenHandler> _logger;

    public IntrospectTokenHandler(
        AppDbContext context,
        IEncryptionService encryptionService,
        ILogger<IntrospectTokenHandler> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<IntrospectTokenResponse> HandleAsync(
        Guid applicationId,
        IntrospectTokenRequest request,
        CancellationToken ct)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == applicationId && !a.IsDeleted, ct);

        if (application == null)
        {
            _logger.LogWarning(
                "Token introspection attempted for non-existent application: {ApplicationId}",
                applicationId);

            return new IntrospectTokenResponse(
                Active: false,
                Sub: null,
                Email: null,
                ClientId: null,
                Exp: null,
                Iat: null,
                Scope: null,
                TokenType: null,
                ApplicationId: null
            );
        }

        string jwtSecret;
        try
        {
            jwtSecret = _encryptionService.Decrypt(application.JwtSecretEncrypted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to decrypt JWT secret for application {ApplicationId}",
                applicationId);

            return InactiveTokenResponse();
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(request.Token))
        {
            _logger.LogWarning(
                "Invalid JWT format provided to introspect endpoint for app {ApplicationId}",
                applicationId);

            return InactiveTokenResponse();
        }

        try
        {
            var key = Encoding.UTF8.GetBytes(jwtSecret);
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

            var principal = tokenHandler.ValidateToken(
                request.Token,
                validationParameters,
                out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var tokenType = principal.FindFirst("token_type")?.Value;
            var userType = principal.FindFirst("user_type")?.Value;
            var appIdClaim = principal.FindFirst("application_id")?.Value;

            if (!string.IsNullOrEmpty(appIdClaim) &&
                Guid.TryParse(appIdClaim, out var tokenAppId) &&
                tokenAppId != applicationId)
            {
                _logger.LogWarning(
                    "Token from application {TokenAppId} used with application {ApplicationId}",
                    tokenAppId,
                    applicationId);

                return InactiveTokenResponse();
            }

            if (userType == "user" && Guid.TryParse(userId, out var parsedUserId))
            {
                var userExists = await _context.Users
                    .AnyAsync(u => u.Id == parsedUserId && u.ApplicationId == applicationId, ct);

                if (!userExists)
                {
                    _logger.LogWarning(
                        "Token introspection for deleted/non-existent user {UserId} in app {ApplicationId}",
                        parsedUserId,
                        applicationId);

                    return InactiveTokenResponse();
                }
            }

            if (tokenType == "refresh" && Guid.TryParse(userId, out var refreshUserId))
            {
                var isRevoked = await _context.UserRefreshTokens
                    .AnyAsync(rt =>
                        rt.UserId == refreshUserId &&
                        rt.Token == request.Token &&
                        rt.IsRevoked, ct);

                if (isRevoked)
                {
                    _logger.LogInformation(
                        "Introspection attempted on revoked refresh token for user {UserId}",
                        refreshUserId);

                    return InactiveTokenResponse();
                }
            }

            var standardClaims = new HashSet<string>
            {
                ClaimTypes.NameIdentifier,
                ClaimTypes.Email,
                ClaimTypes.Role,
                "user_type",
                "token_type",
                "application_id",
                "exp",
                "iat",
                "iss",
                "aud"
            };

            var additionalClaims = principal.Claims
                .Where(c => !standardClaims.Contains(c.Type))
                .ToDictionary(c => c.Type, c => (object)c.Value);

            _logger.LogInformation(
                "Successfully introspected token for user {UserId} in application {ApplicationId}",
                userId,
                applicationId);

            return new IntrospectTokenResponse(
                Active: true,
                Sub: userId,
                Email: email,
                ClientId: application.ClientId,
                Exp: jwtToken.ValidTo != DateTime.MinValue
                    ? new DateTimeOffset(jwtToken.ValidTo).ToUnixTimeSeconds()
                    : null,
                Iat: jwtToken.IssuedAt != DateTime.MinValue
                    ? new DateTimeOffset(jwtToken.IssuedAt).ToUnixTimeSeconds()
                    : null,
                Scope: null,
                TokenType: tokenType,
                ApplicationId: applicationId,
                AdditionalClaims: additionalClaims.Any() ? additionalClaims : null
            );
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogInformation(
                "Expired token provided to introspect endpoint for app {ApplicationId}",
                applicationId);

            return InactiveTokenResponse();
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex,
                "Invalid token provided to introspect endpoint for app {ApplicationId}",
                applicationId);

            return InactiveTokenResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error during token introspection for app {ApplicationId}",
                applicationId);

            return InactiveTokenResponse();
        }
    }

    private static IntrospectTokenResponse InactiveTokenResponse()
    {
        return new IntrospectTokenResponse(
            Active: false,
            Sub: null,
            Email: null,
            ClientId: null,
            Exp: null,
            Iat: null,
            Scope: null,
            TokenType: null,
            ApplicationId: null
        );
    }
}

public static class UserTokenIntrospection
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/apps/{{appId:guid}}/auth/introspect", async (
                Guid appId,
                IntrospectTokenRequest request,
                IntrospectTokenHandler handler,
                CancellationToken ct) =>
            {
                var validator = new IntrospectTokenValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(appId, request, ct);
                return Results.Ok(ApiResponse<IntrospectTokenResponse>.Ok(response));
            })
            .WithName("IntrospectToken")
            .WithTags("Users")
            .AllowAnonymous();
    }
}