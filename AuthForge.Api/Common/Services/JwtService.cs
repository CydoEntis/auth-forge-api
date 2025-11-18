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
    private readonly ILogger<JwtService> _logger;
    private string? _cachedSecret;

    private const int AccessTokenExpirationMinutes = 15;  
    private const int RefreshTokenExpirationDays = 7;     

    public JwtService(IConfigurationService configService, ILogger<JwtService> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    private async Task<string> GetJwtSecretAsync()
    {
        if (_cachedSecret != null)
            return _cachedSecret;

        var config = await _configService.GetAsync();
        
        if (config?.JwtSecretEncrypted == null)
        {
            _logger.LogError("JWT secret not found in configuration");
            throw new InvalidOperationException("JWT secret not configured. Please complete setup.");
        }

        _cachedSecret = config.JwtSecretEncrypted;
        return _cachedSecret;
    }

    public async Task<TokenPair> GenerateAdminTokenPairAsync(Guid adminId, string email)
    {
        var secret = await GetJwtSecretAsync();
        
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays);

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