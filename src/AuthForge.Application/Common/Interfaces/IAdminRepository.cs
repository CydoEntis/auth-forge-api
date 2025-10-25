using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IAdminRepository
{
    Task<Domain.Entities.Admin?> GetByIdAsync(AdminId id, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Admin?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> AnyExistsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Entities.Admin admin, CancellationToken cancellationToken = default);
    void Update(Domain.Entities.Admin admin);
}