using AuthForge.Application.Common.Interfaces;
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AuthForgeDbContext _context;

    public UnitOfWork(AuthForgeDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException(
                "Concurrency error occurred. The entity may have been modified or deleted.", ex);
        }
    }
}