using AuthForge.Application.Common.Interfaces;
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
        var userId = id.Value;
        return await _context.EndUsers
            .FirstOrDefaultAsync(u => u.Id.Value == userId, cancellationToken);
    }


    public async Task<EndUser?> GetByEmailAsync(ApplicationId applicationId, Email email,
        CancellationToken cancellationToken = default)
    {
        var appId = applicationId.Value;
        var emailValue = email.Value;
        return await _context.EndUsers
            .FirstOrDefaultAsync(u => u.ApplicationId.Value == appId && u.Email.Value == emailValue, cancellationToken);
    }

    public async Task<bool> ExistsAsync(ApplicationId applicationId, Email email,
        CancellationToken cancellationToken = default)
    {
        var appId = applicationId.Value;
        var emailValue = email.Value;
        return await _context.EndUsers
            .AnyAsync(u => u.ApplicationId.Value == appId && u.Email.Value == emailValue, cancellationToken);
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

    public Task<List<EndUser>> GetByApplicationAsync(System.ApplicationId applicationId, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<List<EndUser>> GetByApplicationAsync(ApplicationId applicationId, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var appId = applicationId.Value;
        return await _context.EndUsers
            .Where(u => u.ApplicationId.Value == appId)
            .OrderBy(u => u.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}