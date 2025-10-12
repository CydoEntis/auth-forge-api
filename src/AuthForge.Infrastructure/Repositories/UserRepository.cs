using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthForgeDbContext _context;

    public UserRepository(AuthForgeDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(TenantId tenantId, Email email,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsAsync(TenantId tenantId, Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.TenantId == tenantId && u.Email == email, cancellationToken);
    }

    public async Task<List<User>> GetByTenantAsync(TenantId tenantId, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Delete(User user)
    {
        _context.Users.Remove(user);
    }
}