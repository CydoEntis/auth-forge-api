using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthForgeDbContext _context;

    public RefreshTokenRepository(AuthForgeDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<List<RefreshToken>> GetActiveTokensForUserAsync(UserId userId,
        CancellationToken cancellationToken = default)
    {
        var userIdValue = userId.Value;

        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userIdValue && !rt.RevokedAtUtc.HasValue && rt.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RefreshToken>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var userIdValue = userId.Value;

        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userIdValue)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public void Update(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
    }

    public void Delete(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Remove(refreshToken);
    }

    public async Task<bool> ExistsAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .AnyAsync(rt => rt.Token == token, cancellationToken);
    }
}