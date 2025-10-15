using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Infrastructure.Repositories;

public class AuthForgePasswordResetTokenRepository : IAuthForgePasswordResetTokenRepository
{
    private readonly AuthForgeDbContext _context;

    public AuthForgePasswordResetTokenRepository(AuthForgeDbContext context)
    {
        _context = context;
    }

    public async Task<AuthForgePasswordResetToken?> GetByTokenAsync(string token,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuthForgePasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task<List<AuthForgePasswordResetToken>> GetActiveTokensForUserAsync(AuthForgeUserId userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuthForgePasswordResetTokens
            .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AuthForgePasswordResetToken token, CancellationToken cancellationToken = default)
    {
        await _context.AuthForgePasswordResetTokens.AddAsync(token, cancellationToken);
    }

    public void Update(AuthForgePasswordResetToken token)
    {
        _context.AuthForgePasswordResetTokens.Update(token);
    }

    public async Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.AuthForgePasswordResetTokens
            .Where(t => t.ExpiresAtUtc < DateTime.UtcNow || t.IsUsed)
            .ToListAsync(cancellationToken);

        _context.AuthForgePasswordResetTokens.RemoveRange(expiredTokens);
    }
}