using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using App = AuthForge.Domain.Entities.Application;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Infrastructure.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly AuthForgeDbContext _context;

    public ApplicationRepository(AuthForgeDbContext context)
    {
        _context = context;
    }

    public async Task<App?> GetByIdAsync(ApplicationId id, CancellationToken cancellationToken = default)
    {
        var appId = id.Value;
        return await _context.Applications
            .FirstOrDefaultAsync(a => a.Id.Value == appId, cancellationToken);
    }

    public async Task<App?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .FirstOrDefaultAsync(a => a.Slug == slug, cancellationToken);
    }

    public async Task<List<App>> GetByUserIdAsync(AuthForgeUserId userId,
        CancellationToken cancellationToken = default)
    {
        var userIdValue = userId.Value;
        return await _context.Applications
            .Where(a => a.UserId.Value == userIdValue)
            .OrderBy(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .AnyAsync(a => a.Slug == slug, cancellationToken);
    }

    public async Task AddAsync(App application, CancellationToken cancellationToken = default)
    {
        await _context.Applications.AddAsync(application, cancellationToken);
    }

    public void Update(App application)
    {
        _context.Applications.Update(application);
    }

    public void Delete(App application)
    {
        _context.Applications.Remove(application);
    }
}