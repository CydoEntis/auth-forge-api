using AuthForge.Domain.Entities;

namespace AuthForge.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(User user, string? ipAddress = null, string? userAgent = null);
}