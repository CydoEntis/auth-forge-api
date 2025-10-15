using AuthForge.Application.Applications.Models;
using AuthForge.Application.Applications.Queries.GetMy;
using AuthForge.Application.Common.Models;
using App = AuthForge.Domain.Entities.Application;

namespace AuthForge.Application.Applications.Extensions;

public static class ApplicationQueryExtensions
{
    public static List<App> ApplySearch(
        this List<App> applications,
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return applications;

        return applications
            .Where(app => app.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                          app.Slug.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public static List<App> ApplySort(
        this List<App> applications,
        ApplicationSortField sortBy,
        SortOrder sortOrder)
    {
        return sortBy switch
        {
            ApplicationSortField.Name => sortOrder == SortOrder.Asc
                ? applications.OrderBy(a => a.Name).ToList()
                : applications.OrderByDescending(a => a.Name).ToList(),

            ApplicationSortField.Slug => sortOrder == SortOrder.Asc
                ? applications.OrderBy(a => a.Slug).ToList()
                : applications.OrderByDescending(a => a.Slug).ToList(),

            _ => sortOrder == SortOrder.Asc
                ? applications.OrderBy(a => a.CreatedAtUtc).ToList()
                : applications.OrderByDescending(a => a.CreatedAtUtc).ToList()
        };
    }
}