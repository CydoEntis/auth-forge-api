using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> GetActiveTokensForUserAsync(UserId userId, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    void Update(RefreshToken refreshToken);
    void Delete(RefreshToken refreshToken);
}