using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly AuthForgeDbContext _context;

    public TenantRepository(AuthForgeDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(TenantId id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .AnyAsync(t => t.Slug == slug, cancellationToken);
    }

    public async Task<List<Tenant>> GetAllAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .OrderBy(t => t.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(tenant, cancellationToken);
    }

    public void Update(Tenant tenant)
    {
        _context.Tenants.Update(tenant);
    }

    public void Remove(Tenant tenant)
    {
        _context.Tenants.Remove(tenant);
    }
}