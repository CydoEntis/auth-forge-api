using AuthForge.Application.Applications.Enums;
using AuthForge.Application.Common.Extensions;
using AuthForge.Application.Common.Models;

namespace AuthForge.Infrastructure.Repositories.QueryBuilders;

public static class ApplicationQueryBuilder
{
    public static IQueryable<Domain.Entities.Application> ApplyFilters(
        this IQueryable<Domain.Entities.Application> query,
        string? searchTerm,
        bool? isActive)
    {
        query = query.WhereIf(
            !string.IsNullOrWhiteSpace(searchTerm),
            a => a.Name.Contains(searchTerm!) || a.Slug.Contains(searchTerm!));

        query = query.WhereIf(
            isActive.HasValue,
            a => a.IsActive == isActive!.Value);

        return query;
    }

    public static IQueryable<Domain.Entities.Application> ApplySorting(
        this IQueryable<Domain.Entities.Application> query,
        ApplicationSortBy sortBy,
        SortOrder sortOrder)
    {
        return sortBy switch
        {
            ApplicationSortBy.Name => query.OrderByDirection(a => a.Name, sortOrder),
            ApplicationSortBy.Slug => query.OrderByDirection(a => a.Slug, sortOrder),
            ApplicationSortBy.UpdatedAt => query.OrderByDirection(a => a.UpdatedAtUtc, sortOrder),
            _ => query.OrderByDirection(a => a.CreatedAtUtc, sortOrder)
        };
    }
}