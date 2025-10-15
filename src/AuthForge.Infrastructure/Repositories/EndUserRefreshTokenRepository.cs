using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Infrastructure.Repositories;

public class EndUserRefreshTokenRepository : IEndUserRefreshTokenRepository
{
    private readonly AuthForgeDbContext _context;

    public EndUserRefreshTokenRepository(AuthForgeDbContext context)
    {
        _context = context;
    }

    public async Task<EndUserRefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EndUserRefreshTokens
            .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
    }

    public async Task<EndUserRefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.EndUserRefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<List<EndUserRefreshToken>> GetActiveTokensForUserAsync(EndUserId userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.EndUserRefreshTokens
            .Where(rt => rt.UserId == userId && !rt.RevokedAtUtc.HasValue && rt.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EndUserRefreshToken>> GetByUserIdAsync(EndUserId userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.EndUserRefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(EndUserRefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.EndUserRefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public void Update(EndUserRefreshToken refreshToken)
    {
        _context.EndUserRefreshTokens.Update(refreshToken);
    }

    public void Delete(EndUserRefreshToken refreshToken)
    {
        _context.EndUserRefreshTokens.Remove(refreshToken);
    }

    public async Task<bool> ExistsAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.EndUserRefreshTokens
            .AnyAsync(rt => rt.Token == token, cancellationToken);
    }
}