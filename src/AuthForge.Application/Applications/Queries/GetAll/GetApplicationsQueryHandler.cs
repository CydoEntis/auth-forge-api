using AuthForge.Application.Applications.Enums;
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

        var pageNumber = parameters.PageNumber ?? 1;
        var pageSize = parameters.PageSize ?? 10;
        var sortBy = parameters.SortBy ?? ApplicationSortBy.CreatedAt;
        var sortOrder = parameters.SortOrder ?? SortOrder.Desc;

        var (items, totalCount) = await _applicationRepository.GetPagedAsync(
            parameters.SearchTerm,
            parameters.IsActive,
            sortBy,
            sortOrder,
            pageNumber,
            pageSize,
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

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new PagedResponse<ApplicationSummary>
        {
            Items = applications,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };

        return Result<PagedResponse<ApplicationSummary>>.Success(response);
    }
}