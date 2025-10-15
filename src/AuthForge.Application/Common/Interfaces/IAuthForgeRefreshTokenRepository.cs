using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IAuthForgeRefreshTokenRepository
{
    Task<AuthForgeRefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AuthForgeRefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<List<AuthForgeRefreshToken>> GetActiveTokensForUserAsync(AuthForgeUserId userId,
        CancellationToken cancellationToken = default);

    Task<List<AuthForgeRefreshToken>> GetByUserIdAsync(AuthForgeUserId userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(AuthForgeRefreshToken refreshToken, CancellationToken cancellationToken = default);
    void Update(AuthForgeRefreshToken refreshToken);
    void Delete(AuthForgeRefreshToken refreshToken);
    Task<bool> ExistsAsync(string token, CancellationToken cancellationToken = default);
}