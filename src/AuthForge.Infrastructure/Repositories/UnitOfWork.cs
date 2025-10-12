using AuthForge.Application.Common.Interfaces;
using AuthForge.Infrastructure.Data;

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
        return await _context.SaveChangesAsync(cancellationToken);
    }
}