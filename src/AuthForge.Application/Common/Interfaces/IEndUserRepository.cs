using AuthForge.Application.Common.Models;
using AuthForge.Application.EndUsers.Enums;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Common.Interfaces;

public interface IEndUserRepository
{
    Task<EndUser?> GetByIdAsync(EndUserId id, CancellationToken cancellationToken = default);

    Task<EndUser?> GetByEmailAsync(ApplicationId applicationId, Email email,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(ApplicationId applicationId, Email email, CancellationToken cancellationToken = default);
    Task AddAsync(EndUser user, CancellationToken cancellationToken = default);
    void Update(EndUser user);
    void Delete(EndUser user);
    
    Task<List<EndUser>> GetByApplicationAsync(ApplicationId applicationId, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default);

    Task<(List<EndUser> Items, int TotalCount)> GetPagedAsync(
        ApplicationId applicationId,
        string? searchTerm,
        bool? isActive,
        bool? isEmailVerified,
        EndUserSortBy sortBy,
        SortOrder sortOrder,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}