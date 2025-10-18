using AuthForge.Application.Applications.Models;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Applications.Queries.GetAll;

public sealed class GetApplicationsQueryHandler
    : IQueryHandler<GetApplicationsQuery, Result<PagedResponse<ApplicationSummary>>>
{
    private readonly IApplicationRepository _applicationRepository;

    public GetApplicationsQueryHandler(IApplicationRepository applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }

    public async ValueTask<Result<PagedResponse<ApplicationSummary>>> Handle(
        GetApplicationsQuery query,
        CancellationToken cancellationToken)
    {
        var parameters = query.Parameters;

        var (items, totalCount) = await _applicationRepository.GetPagedAsync(
            parameters.SearchTerm,
            parameters.IsActive,
            parameters.SortBy,
            parameters.SortOrder,
            parameters.PageNumber,
            parameters.PageSize,
            cancellationToken);

        var applications = items.Select(app => new ApplicationSummary(
            app.Id.Value.ToString(),
            app.Name,
            app.Slug,
            app.PublicKey,
            app.IsActive,
            0,
            app.CreatedAtUtc
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

        var response = new PagedResponse<ApplicationSummary>
        {
            Items = applications,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalPages = totalPages
        };

        return Result<PagedResponse<ApplicationSummary>>.Success(response);
    }
}