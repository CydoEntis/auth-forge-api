using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    Task<EndUserRefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<EndUserRefreshToken>> GetActiveTokensForUserAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<List<EndUserRefreshToken>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task AddAsync(EndUserRefreshToken endUserRefreshToken, CancellationToken cancellationToken = default);
    void Update(EndUserRefreshToken endUserRefreshToken);
    void Delete(EndUserRefreshToken endUserRefreshToken);
}