using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Infrastructure.Repositories;

public class EndUserPasswordResetTokenRepository : IEndUserPasswordResetTokenRepository
{
    private readonly AuthForgeDbContext _context;

    public EndUserPasswordResetTokenRepository(AuthForgeDbContext context)
    {
        _context = context;
    }

    public async Task<EndUserPasswordResetToken?> GetByTokenAsync(string token,
        CancellationToken cancellationToken = default)
    {
        return await _context.EndUserPasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task<List<EndUserPasswordResetToken>> GetActiveTokensForUserAsync(EndUserId userId,
        CancellationToken cancellationToken = default)
    {
        var userIdValue = userId.Value;
        return await _context.EndUserPasswordResetTokens
            .Where(t => t.UserId.Value == userIdValue && !t.IsUsed && t.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(EndUserPasswordResetToken token, CancellationToken cancellationToken = default)
    {
        await _context.EndUserPasswordResetTokens.AddAsync(token, cancellationToken);
    }

    public void Update(EndUserPasswordResetToken token)
    {
        _context.EndUserPasswordResetTokens.Update(token);
    }

    public async Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.EndUserPasswordResetTokens
            .Where(t => t.ExpiresAtUtc < DateTime.UtcNow || t.IsUsed)
            .ToListAsync(cancellationToken);

        _context.EndUserPasswordResetTokens.RemoveRange(expiredTokens);
    }
}