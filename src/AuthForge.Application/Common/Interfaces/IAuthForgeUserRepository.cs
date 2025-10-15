using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IAuthForgeUserRepository
{
    Task<AuthForgeUser?> GetByIdAsync(AuthForgeUserId id, CancellationToken cancellationToken = default);
    Task<AuthForgeUser?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default);
    Task AddAsync(AuthForgeUser user, CancellationToken cancellationToken = default);
    void Update(AuthForgeUser user);
    void Delete(AuthForgeUser user);
}