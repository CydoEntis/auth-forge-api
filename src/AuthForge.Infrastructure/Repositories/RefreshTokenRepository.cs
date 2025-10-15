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

    public async Task<EndUserRefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
    }

    public async Task<EndUserRefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<List<EndUserRefreshToken>> GetActiveTokensForUserAsync(UserId userId,
        CancellationToken cancellationToken = default)
    {
        var userIdValue = userId.Value;

        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userIdValue && !rt.RevokedAtUtc.HasValue && rt.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EndUserRefreshToken>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var userIdValue = userId.Value;

        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userIdValue)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(EndUserRefreshToken endUserRefreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(endUserRefreshToken, cancellationToken);
    }

    public void Update(EndUserRefreshToken endUserRefreshToken)
    {
        _context.RefreshTokens.Update(endUserRefreshToken);
    }

    public void Delete(EndUserRefreshToken endUserRefreshToken)
    {
        _context.RefreshTokens.Remove(endUserRefreshToken);
    }

    public async Task<bool> ExistsAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .AnyAsync(rt => rt.Token == token, cancellationToken);
    }
}