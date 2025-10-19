using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Application.EndUsers.Enums;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Infrastructure.Repositories;

public class EndUserRepository : IEndUserRepository
{
    private readonly AuthForgeDbContext _context;

    public EndUserRepository(AuthForgeDbContext context)
    {
        _context = context;
    }

    public async Task<EndUser?> GetByIdAsync(EndUserId id, CancellationToken cancellationToken = default)
    {
        return await _context.EndUsers
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<EndUser?> GetByEmailAsync(ApplicationId applicationId, Email email,
        CancellationToken cancellationToken = default)
    {
        return await _context.EndUsers
            .FirstOrDefaultAsync(u => u.ApplicationId == applicationId && u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsAsync(ApplicationId applicationId, Email email,
        CancellationToken cancellationToken = default)
    {
        return await _context.EndUsers
            .AnyAsync(u => u.ApplicationId == applicationId && u.Email == email, cancellationToken);
    }

    public async Task AddAsync(EndUser user, CancellationToken cancellationToken = default)
    {
        await _context.EndUsers.AddAsync(user, cancellationToken);
    }

    public void Update(EndUser user)
    {
        _context.EndUsers.Update(user);
    }

    public void Delete(EndUser user)
    {
        _context.EndUsers.Remove(user);
    }

    public async Task<List<EndUser>> GetByApplicationAsync(ApplicationId applicationId, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.EndUsers
            .Where(u => u.ApplicationId == applicationId)
            .OrderBy(u => u.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<EndUser> Items, int TotalCount)> GetPagedAsync(
        ApplicationId applicationId,
        string? searchTerm,
        bool? isActive,
        EndUserSortBy sortBy,
        SortOrder sortOrder,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.EndUsers
            .Where(u => u.ApplicationId == applicationId);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u =>
                u.Email.Value.Contains(searchTerm) ||
                (u.FirstName != null && u.FirstName.Contains(searchTerm)) ||
                (u.LastName != null && u.LastName.Contains(searchTerm)));
        }

        // Apply IsActive filter
        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = sortBy switch
        {
            EndUserSortBy.Email => sortOrder == SortOrder.Asc
                ? query.OrderBy(u => u.Email.Value)
                : query.OrderByDescending(u => u.Email.Value),
            EndUserSortBy.FirstName => sortOrder == SortOrder.Asc
                ? query.OrderBy(u => u.FirstName)
                : query.OrderByDescending(u => u.FirstName),
            EndUserSortBy.LastName => sortOrder == SortOrder.Asc
                ? query.OrderBy(u => u.LastName)
                : query.OrderByDescending(u => u.LastName),
            EndUserSortBy.LastLoginAt => sortOrder == SortOrder.Asc
                ? query.OrderBy(u => u.LastLoginAtUtc)
                : query.OrderByDescending(u => u.LastLoginAtUtc),
            EndUserSortBy.CreatedAt or _ => sortOrder == SortOrder.Asc
                ? query.OrderBy(u => u.CreatedAtUtc)
                : query.OrderByDescending(u => u.CreatedAtUtc)
        };

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}