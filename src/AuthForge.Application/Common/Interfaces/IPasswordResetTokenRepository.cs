using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<PasswordResetToken>> GetActiveTokensForUserAsync(UserId userId, CancellationToken cancellationToken = default);
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    void Update(PasswordResetToken token);
    Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
}