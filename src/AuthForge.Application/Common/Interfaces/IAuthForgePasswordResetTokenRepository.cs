using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IAuthForgePasswordResetTokenRepository
{
    Task<AuthForgePasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<List<AuthForgePasswordResetToken>> GetActiveTokensForUserAsync(AuthForgeUserId userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(AuthForgePasswordResetToken token, CancellationToken cancellationToken = default);
    void Update(AuthForgePasswordResetToken token);
    Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
}