using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Infrastructure.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly AuthForgeDbContext _context;

    public AdminRepository(AuthForgeDbContext context)
    {
        _context = context;
    }

    public async Task<Admin?> GetByIdAsync(AdminId id, CancellationToken cancellationToken = default)
    {
        return await _context.Admins
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Admin?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Admins
            .FirstOrDefaultAsync(a => a.Email.Value == email.Value, cancellationToken);
    }

    public async Task<bool> AnyExistsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Admins.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Admin admin, CancellationToken cancellationToken = default)
    {
        await _context.Admins.AddAsync(admin, cancellationToken);
    }

    public void Update(Admin admin)
    {
        _context.Admins.Update(admin);
    }
}