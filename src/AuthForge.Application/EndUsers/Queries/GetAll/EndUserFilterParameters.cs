using AuthForge.Application.Common.Models;
using AuthForge.Application.EndUsers.Enums;

namespace AuthForge.Application.EndUsers.Queries.GetAll;

public sealed record EndUserFilterParameters : PagedQuery
{
    public bool? IsActive { get; init; }
    public EndUserSortBy? SortBy { get; init; }
    public SortOrder? SortOrder { get; init; }
}