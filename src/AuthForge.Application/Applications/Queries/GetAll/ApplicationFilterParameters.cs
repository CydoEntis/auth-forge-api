using AuthForge.Application.Applications.Enums;
using AuthForge.Application.Common.Models;

namespace AuthForge.Application.Applications.Queries.GetAll;

public sealed record ApplicationFilterParameters : PagedQuery
{
    public bool? IsActive { get; init; }
    public ApplicationSortBy? SortBy { get; init; }
    public SortOrder? SortOrder { get; init; }
}