using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Features.Shared.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuthForge.Api.Common.Services;

public class JwtService : IJwtService
{
    private readonly IConfigurationService _configService;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<JwtService> _logger;
    private string? _cachedAdminSecret;

    private const int DefaultAccessTokenExpirationMinutes = 15;
    private const int DefaultRefreshTokenExpirationDays = 7;

    public JwtService(
        IConfigurationService configService,
        IEncryptionService encryptionService,
        ILogger<JwtService> logger)
    {
        _configService = configService;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    private async Task<string> GetAdminJwtSecretAsync()
    {
        if (_cachedAdminSecret != null)
            return _cachedAdminSecret;

        var config = await _configService.GetAsync();

        if (config?.JwtSecretEncrypted == null)
        {
            _logger.LogError("JWT secret not found in configuration");
            throw new InvalidOperationException("JWT secret not configured. Please complete setup.");
        }

        _cachedAdminSecret = config.JwtSecretEncrypted;
        return _cachedAdminSecret;
    }

    public async Task<TokenPair> GenerateAdminTokenPairAsync(Guid adminId, string email)
    {
        var secret = await GetAdminJwtSecretAsync();

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(DefaultAccessTokenExpirationMinutes);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(DefaultRefreshTokenExpirationDays);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim("user_type", "admin"),
            new Claim("token_type", "access")
        };

        var accessToken = GenerateJwtToken(claims, secret, accessTokenExpiresAt);
        var refreshToken = GenerateRefreshToken();

        _logger.LogInformation("Generated token pair for admin {AdminId}", adminId);

        return new TokenPair(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            AccessTokenExpiresAt: accessTokenExpiresAt,
            RefreshTokenExpiresAt: refreshTokenExpiresAt
        );
    }

    public Task<TokenPair> GenerateUserTokenPairAsync(
        Guid userId,
        string email,
        Guid applicationId,
        string appJwtSecretEncrypted,
        int accessTokenExpirationMinutes,
        int refreshTokenExpirationDays)
    {
        var secret = _encryptionService.Decrypt(appJwtSecretEncrypted);

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, "user"),
            new Claim("user_type", "user"),
            new Claim("application_id", applicationId.ToString()),
            new Claim("token_type", "access")
        };

        var accessToken = GenerateJwtToken(claims, secret, accessTokenExpiresAt);
        var refreshToken = GenerateRefreshToken();

        _logger.LogInformation(
            "Generated token pair for user {UserId} in application {ApplicationId}",
            userId,
            applicationId);

        return Task.FromResult(new TokenPair(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            AccessTokenExpiresAt: accessTokenExpiresAt,
            RefreshTokenExpiresAt: refreshTokenExpiresAt
        ));
    }

    private string GenerateJwtToken(List<Claim> claims, string secret, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "AuthForge",
            audience: "AuthForge",
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}