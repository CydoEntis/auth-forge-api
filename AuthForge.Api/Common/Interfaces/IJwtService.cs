using AuthForge.Api.Features.Shared.Models;

namespace AuthForge.Api.Common.Interfaces;

public interface IJwtService
{
    Task<TokenPair> GenerateAdminTokenPairAsync(Guid adminId, string email);

    Task<TokenPair> GenerateUserTokenPairAsync(
        Guid userId,
        string email,
        Guid applicationId,
        string appJwtSecretEncrypted,
        int accessTokenExpirationMinutes,
        int refreshTokenExpirationDays);
}