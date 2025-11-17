using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Infrastructure.Data;
using Microsoft.IdentityModel.Tokens;

namespace AuthForge.Infrastructure.Authentication;

public class AdminJwtTokenGenerator : IAdminJwtTokenGenerator
{
    private readonly ConfigurationDatabase _configDb;
    private const int DefaultAccessTokenExpirationMinutes = 15;

    public AdminJwtTokenGenerator(ConfigurationDatabase configDb)
    {
        _configDb = configDb;
    }

    public string GenerateAccessToken(string email)
    {
        var settings = _configDb.GetAllAsync().GetAwaiter().GetResult();
        var jwtSecret = settings.GetValueOrDefault("jwt_secret");
        var jwtIssuer = settings.GetValueOrDefault("jwt_issuer", "AuthForge");
        var jwtAudience = settings.GetValueOrDefault("jwt_audience", "AuthForgeClient");

        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new InvalidOperationException("JWT secret not found in configuration database");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("role", "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(DefaultAccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
