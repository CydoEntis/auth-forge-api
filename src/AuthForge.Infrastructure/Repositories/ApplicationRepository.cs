using AuthForge.Application.Applications.Enums;
using AuthForge.Application.Common.Extensions;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
using AuthForge.Infrastructure.Repositories.QueryBuilders;
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
        return await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<App?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .FirstOrDefaultAsync(a => a.Slug == slug, cancellationToken);
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

    public async Task<(List<App> Items, int TotalCount)> GetPagedAsync(
        string? searchTerm,
        bool? isActive,
        ApplicationSortBy sortBy,
        SortOrder sortOrder,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Applications
            .AsQueryable()
            .ApplyFilters(searchTerm, isActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplySorting(sortBy, sortOrder)
            .Paginate(pageNumber, pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<App?> GetByPublicKeyAsync(
        string publicKey,
        CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .FirstOrDefaultAsync(a => a.PublicKey == publicKey, cancellationToken);
    }
}