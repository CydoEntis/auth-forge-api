using AuthForge.Api.Common;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Applications.Shared.Models;
using AuthForge.Api.Features.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record ListApplicationsRequest(
    string? Search = null,
    bool? IsActive = null,
    ApplicationSortField SortBy = ApplicationSortField.CreatedAt,
    SortOrder SortOrder = SortOrder.Desc,
    int Page = 1,
    int PageSize = 10
);

public sealed record ApplicationListItem(
    Guid Id,
    string Name,
    string Slug,
    string ClientId,
    bool IsActive,
    int UserCount,
    ApplicationAuthMethods AuthMethods,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public sealed record ApplicationAuthMethods(
    bool BasicAuth,
    bool GoogleOAuth,
    bool GithubOAuth,
    bool MagicLinks
);

public sealed record ListApplicationsResponse(
    List<ApplicationListItem> Applications,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public class ListApplicationsHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<ListApplicationsHandler> _logger;

    public ListApplicationsHandler(AppDbContext context, ILogger<ListApplicationsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ListApplicationsResponse> HandleAsync(
        ListApplicationsRequest request,
        CancellationToken ct)
    {
        var query = _context.Applications.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(a =>
                a.Name.ToLower().Contains(searchLower) ||
                a.Slug.ToLower().Contains(searchLower) ||
                (a.Description != null && a.Description.ToLower().Contains(searchLower))
            );
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(a => a.IsActive == request.IsActive.Value);
        }

        query = ApplySorting(query, request.SortBy, request.SortOrder);

        var totalCount = await query.CountAsync(ct);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var applications = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(a => new ApplicationListItem(
                a.Id,
                a.Name,
                a.Slug,
                a.ClientId,
                a.IsActive,
                a.Users.Count,
                new ApplicationAuthMethods(
                    true,
                    a.GoogleEnabled,
                    a.GithubEnabled,
                    false
                ),
                a.CreatedAtUtc,
                a.UpdatedAtUtc
            ))
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        _logger.LogInformation(
            "Listed {Count} applications (Page {Page} of {TotalPages})",
            applications.Count, page, totalPages);

        return new ListApplicationsResponse(
            applications,
            totalCount,
            page,
            pageSize,
            totalPages
        );
    }

    private static IQueryable<Entities.Application> ApplySorting(
        IQueryable<Entities.Application> query,
        ApplicationSortField sortBy,
        SortOrder sortOrder)
    {
        var isDescending = sortOrder == SortOrder.Desc;

        return sortBy switch
        {
            ApplicationSortField.Name => isDescending
                ? query.OrderByDescending(a => a.Name)
                : query.OrderBy(a => a.Name),

            ApplicationSortField.Slug => isDescending
                ? query.OrderByDescending(a => a.Slug)
                : query.OrderBy(a => a.Slug),

            ApplicationSortField.IsActive => isDescending
                ? query.OrderByDescending(a => a.IsActive)
                : query.OrderBy(a => a.IsActive),

            ApplicationSortField.UpdatedAt => isDescending
                ? query.OrderByDescending(a => a.UpdatedAtUtc)
                : query.OrderBy(a => a.UpdatedAtUtc),

            ApplicationSortField.CreatedAt or _ => isDescending
                ? query.OrderByDescending(a => a.CreatedAtUtc)
                : query.OrderBy(a => a.CreatedAtUtc),
        };
    }
}

public static class ListApplications
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}/applications", async (
                [AsParameters] ListApplicationsRequest request,
                [FromServices] ListApplicationsHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<ListApplicationsResponse>.Ok(response));
            })
            .WithName("ListApplications")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}