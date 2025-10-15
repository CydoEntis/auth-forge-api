using AuthForge.Application.Applications.Models;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Applications.Queries.GetMy;

public sealed record GetMyApplicationsQuery : PagedQuery, IQuery<Result<PagedResponse<ApplicationSummary>>>
{
    public required string UserId { get; init; }
    public ApplicationSortField SortBy { get; init; } = ApplicationSortField.CreatedAt;
    public SortOrder SortOrder { get; init; } = SortOrder.Desc;
}