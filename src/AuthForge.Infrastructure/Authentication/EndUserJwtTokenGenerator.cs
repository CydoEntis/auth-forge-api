using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
using Microsoft.IdentityModel.Tokens;
using App = AuthForge.Domain.Entities.Application;

namespace AuthForge.Infrastructure.Authentication;

public sealed class EndUserJwtTokenGenerator : IEndUserJwtTokenGenerator
{
    private readonly ConfigurationDatabase _configDb;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public EndUserJwtTokenGenerator(ConfigurationDatabase configDb)
    {
        _configDb = configDb;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(EndUser user, App application)
    {
        // Read JWT settings from configuration database
        var settings = _configDb.GetAllAsync().GetAwaiter().GetResult();
        var jwtSecret = settings.GetValueOrDefault("jwt_secret");
        var jwtIssuer = settings.GetValueOrDefault("jwt_issuer", "AuthForge");
        var jwtAudience = settings.GetValueOrDefault("jwt_audience", "AuthForgeClient");

        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new InvalidOperationException("JWT secret not found in configuration database");
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(application.Settings.AccessTokenExpirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("app_id", application.Id.Value.ToString()),
            new Claim("app_slug", application.Slug)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = _tokenHandler.WriteToken(token);

        return (accessToken, expiresAt);
    }

    public TokenPair GenerateTokenPair(
        EndUser user,
        App application,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var (accessToken, accessTokenExpiresAt) = GenerateAccessToken(user, application);

        var refreshToken = GenerateSecureRefreshToken();

        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(application.Settings.RefreshTokenExpirationDays);

        return new TokenPair(
            accessToken,
            refreshToken,
            accessTokenExpiresAt,
            refreshTokenExpiresAt);
    }

    public EndUserId? ValidateToken(string token)
    {
        try
        {
            // Read JWT settings from configuration database
            var settings = _configDb.GetAllAsync().GetAwaiter().GetResult();
            var jwtSecret = settings.GetValueOrDefault("jwt_secret");
            var jwtIssuer = settings.GetValueOrDefault("jwt_issuer", "AuthForge");
            var jwtAudience = settings.GetValueOrDefault("jwt_audience", "AuthForgeClient");

            if (string.IsNullOrEmpty(jwtSecret))
            {
                return null;
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }

            return EndUserId.Create(userId);
        }
        catch
        {
            return null;
        }
    }

    private static string GenerateSecureRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}