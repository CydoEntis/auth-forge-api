using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using ApplicationId = System.ApplicationId;

namespace AuthForge.Application.Common.Interfaces;

public interface IEndUserRepository
{
    Task<EndUser?> GetByIdAsync(EndUserId id, CancellationToken cancellationToken = default);
    Task<EndUser?> GetByEmailAsync(ApplicationId applicationId, Email email, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(ApplicationId applicationId, Email email, CancellationToken cancellationToken = default);
    Task AddAsync(EndUser user, CancellationToken cancellationToken = default);
    void Update(EndUser user);
    void Delete(EndUser user);
    Task<List<EndUser>> GetByApplicationAsync(ApplicationId applicationId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}