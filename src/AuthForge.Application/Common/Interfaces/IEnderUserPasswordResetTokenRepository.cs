using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IEndUserPasswordResetTokenRepository
{
    Task<EndUserPasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<EndUserPasswordResetToken>> GetActiveTokensForUserAsync(EndUserId userId, CancellationToken cancellationToken = default);
    Task AddAsync(EndUserPasswordResetToken token, CancellationToken cancellationToken = default);
    void Update(EndUserPasswordResetToken token);
    Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
}