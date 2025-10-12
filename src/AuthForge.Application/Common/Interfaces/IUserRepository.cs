using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(TenantId tenantId, Email email, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(TenantId tenantId, Email email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
    void Delete(User user);
    Task<List<User>> GetByTenantAsync(TenantId tenantId, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default);
}