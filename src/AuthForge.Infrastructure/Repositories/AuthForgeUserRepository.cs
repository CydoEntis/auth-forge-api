using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Infrastructure.Repositories;

public class AuthForgeUserRepository : IAuthForgeUserRepository
{
    private readonly AuthForgeDbContext _context;

    public AuthForgeUserRepository(AuthForgeDbContext context)
    {
        _context = context;
    }

    public async Task<AuthForgeUser?> GetByIdAsync(AuthForgeUserId id, CancellationToken cancellationToken = default)
    {
        return await _context.AuthForgeUsers
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<AuthForgeUser?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.AuthForgeUsers
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.AuthForgeUsers
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task AddAsync(AuthForgeUser user, CancellationToken cancellationToken = default)
    {
        await _context.AuthForgeUsers.AddAsync(user, cancellationToken);
    }

    public void Update(AuthForgeUser user)
    {
        _context.AuthForgeUsers.Update(user);
    }

    public void Delete(AuthForgeUser user)
    {
        _context.AuthForgeUsers.Remove(user);
    }
}