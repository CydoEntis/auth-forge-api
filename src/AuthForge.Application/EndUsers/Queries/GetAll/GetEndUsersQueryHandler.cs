using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Application.EndUsers.Enums;
using AuthForge.Application.EndUsers.Models;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.EndUsers.Queries.GetAll;

public sealed class GetEndUsersQueryHandler
    : IQueryHandler<GetEndUsersQuery, Result<PagedResponse<EndUserSummary>>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IApplicationRepository _applicationRepository;

    public GetEndUsersQueryHandler(
        IEndUserRepository endUserRepository,
        IApplicationRepository applicationRepository)
    {
        _endUserRepository = endUserRepository;
        _applicationRepository = applicationRepository;
    }

    public async ValueTask<Result<PagedResponse<EndUserSummary>>> Handle(
        GetEndUsersQuery query,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(query.ApplicationId, out var appGuid))
            return Result<PagedResponse<EndUserSummary>>.Failure(
                ValidationErrors.InvalidGuid("ApplicationId"));

        var applicationId = ApplicationId.Create(appGuid);
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);

        if (application is null)
            return Result<PagedResponse<EndUserSummary>>.Failure(ApplicationErrors.NotFound);

        var parameters = query.Parameters;
        var pageNumber = parameters.PageNumber ?? 1;
        var pageSize = parameters.PageSize ?? 10;

        var (items, totalCount) = await _endUserRepository.GetPagedAsync(
            applicationId,
            parameters.SearchTerm,
            parameters.IsActive,
            parameters.IsEmailVerified,
            parameters.SortBy ?? EndUserSortBy.CreatedAt,
            parameters.SortOrder ?? SortOrder.Desc,
            pageNumber,
            pageSize,
            cancellationToken);

        var endUsers = items.Select(user => new EndUserSummary(
            user.Id.Value.ToString(),
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.IsEmailVerified,
            user.IsActive,
            user.FailedLoginAttempts,
            user.LockedOutUntil,
            user.CreatedAtUtc,
            user.LastLoginAtUtc
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new PagedResponse<EndUserSummary>
        {
            Items = endUsers,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };

        return Result<PagedResponse<EndUserSummary>>.Success(response);
    }
}