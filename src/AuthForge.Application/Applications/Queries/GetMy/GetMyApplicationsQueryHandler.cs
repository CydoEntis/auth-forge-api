using AuthForge.Application.Applications.Extensions;
using AuthForge.Application.Applications.Models;
using AuthForge.Application.Common.Extensions;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.Applications.Queries.GetMy;

public sealed class GetMyApplicationsQueryHandler
    : IQueryHandler<GetMyApplicationsQuery, Result<PagedResponse<ApplicationSummary>>>
{
    private readonly IApplicationRepository _applicationRepository;

    public GetMyApplicationsQueryHandler(IApplicationRepository applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }

    public async ValueTask<Result<PagedResponse<ApplicationSummary>>> Handle(
        GetMyApplicationsQuery query,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(query.UserId, out var userGuid))
            return Result<PagedResponse<ApplicationSummary>>.Failure(ValidationErrors.InvalidGuid("UserId"));

        var userId = AuthForgeUserId.Create(userGuid);

        var applications = await _applicationRepository.GetByUserIdAsync(userId, cancellationToken);

        applications = applications
            .ApplySearch(query.SearchTerm)
            .ApplySort(query.SortBy, query.SortOrder);

        var totalCount = applications.Count;

        var paginatedApps = applications.Paginate(query.PageNumber, query.PageSize);

        var applicationSummaries = paginatedApps.Select(app => new ApplicationSummary(
            app.Id.Value.ToString(),
            app.Name,
            app.Slug,
            app.IsActive,
            0, // TODO: Query EndUser count
            app.CreatedAtUtc)).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var response = new PagedResponse<ApplicationSummary>
        {
            Items = applicationSummaries,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = totalPages
        };

        return Result<PagedResponse<ApplicationSummary>>.Success(response);
    }
}