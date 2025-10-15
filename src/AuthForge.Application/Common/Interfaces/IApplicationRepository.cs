using AuthForge.Domain.ValueObjects;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;
using App = AuthForge.Domain.Entities.Application;

namespace AuthForge.Application.Common.Interfaces;

public interface IApplicationRepository
{
    Task<App?> GetByIdAsync(ApplicationId id, CancellationToken cancellationToken = default);
    Task<App?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<App>> GetByUserIdAsync(AuthForgeUserId userId, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task AddAsync(App application, CancellationToken cancellationToken = default);
    void Update(App application);
    void Delete(App application);
}