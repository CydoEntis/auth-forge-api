using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Infrastructure.Repositories;

public class AuthForgeRefreshTokenRepository : IAuthForgeRefreshTokenRepository
{
    private readonly AuthForgeDbContext _context;

    public AuthForgeRefreshTokenRepository(AuthForgeDbContext context)
    {
        _context = context;
    }

    public async Task<AuthForgeRefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuthForgeRefreshTokens
            .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
    }

    public async Task<AuthForgeRefreshToken?> GetByTokenAsync(string token,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuthForgeRefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<List<AuthForgeRefreshToken>> GetActiveTokensForUserAsync(AuthForgeUserId userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuthForgeRefreshTokens
            .Where(rt => rt.UserId == userId && !rt.RevokedAtUtc.HasValue && rt.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuthForgeRefreshToken>> GetByUserIdAsync(AuthForgeUserId userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuthForgeRefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync(cancellationToken);
    }


    public async Task AddAsync(AuthForgeRefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.AuthForgeRefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public void Update(AuthForgeRefreshToken refreshToken)
    {
        _context.AuthForgeRefreshTokens.Update(refreshToken);
    }

    public void Delete(AuthForgeRefreshToken refreshToken)
    {
        _context.AuthForgeRefreshTokens.Remove(refreshToken);
    }

    public async Task<bool> ExistsAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.AuthForgeRefreshTokens
            .AnyAsync(rt => rt.Token == token, cancellationToken);
    }
}