using AuthForge.Application.Applications.Enums;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.ValueObjects;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;
using App = AuthForge.Domain.Entities.Application;

namespace AuthForge.Application.Common.Interfaces;

public interface IApplicationRepository
{
    Task<App?> GetByIdAsync(ApplicationId id, CancellationToken cancellationToken = default);
    Task<App?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task AddAsync(App application, CancellationToken cancellationToken = default);
    void Update(App application);
    void Delete(App application);

    Task<(List<App> Items, int TotalCount)> GetPagedAsync(
        string? searchTerm,
        bool? isActive,
        ApplicationSortBy sortBy,
        SortOrder sortOrder,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    
    Task<App?> GetByPublicKeyAsync(
        string publicKey,
        CancellationToken cancellationToken = default);
}