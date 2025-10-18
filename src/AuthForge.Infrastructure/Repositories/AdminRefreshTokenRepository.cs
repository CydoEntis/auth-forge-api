using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Infrastructure.Repositories;

public class AdminRefreshTokenRepository : IAdminRefreshTokenRepository
{
    private readonly AuthForgeDbContext _context;

    public AdminRefreshTokenRepository(AuthForgeDbContext context)
    {
        _context = context;
    }

    public async Task<AdminRefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdminRefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task AddAsync(
        AdminRefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        await _context.AdminRefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public void Update(AdminRefreshToken refreshToken)
    {
        _context.AdminRefreshTokens.Update(refreshToken);
    }
}