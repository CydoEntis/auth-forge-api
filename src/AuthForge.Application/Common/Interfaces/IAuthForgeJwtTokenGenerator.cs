using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IAuthForgeJwtTokenGenerator
{
    (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(AuthForgeUser user);

    TokenPair GenerateTokenPair(
        AuthForgeUser user,
        string? ipAddress = null,
        string? userAgent = null);

    AuthForgeUserId? ValidateToken(string token);
}