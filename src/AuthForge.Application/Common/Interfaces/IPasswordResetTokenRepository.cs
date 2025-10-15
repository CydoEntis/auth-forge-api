using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<EndUserPasswordResetToken> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<EndUserPasswordResetToken>> GetActiveTokensForUserAsync(UserId userId, CancellationToken cancellationToken = default);
    Task AddAsync(EndUserPasswordResetToken token, CancellationToken cancellationToken = default);
    void Update(EndUserPasswordResetToken token);
    Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
}