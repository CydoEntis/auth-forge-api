using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{

    (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(User user, Tenant tenant);
    
    TokenPair GenerateTokenPair(
        User user,
        Tenant tenant,
        string? ipAddress = null,
        string? userAgent = null);

    UserId? ValidateToken(string token);
}