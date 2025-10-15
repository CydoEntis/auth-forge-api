using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IEndUserRefreshTokenRepository
{
    Task<EndUserRefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EndUserRefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<List<EndUserRefreshToken>> GetActiveTokensForUserAsync(EndUserId userId,
        CancellationToken cancellationToken = default);

    Task<List<EndUserRefreshToken>> GetByUserIdAsync(EndUserId userId, CancellationToken cancellationToken = default);
    Task AddAsync(EndUserRefreshToken refreshToken, CancellationToken cancellationToken = default);
    void Update(EndUserRefreshToken refreshToken);
    void Delete(EndUserRefreshToken refreshToken);
    Task<bool> ExistsAsync(string token, CancellationToken cancellationToken = default);
}