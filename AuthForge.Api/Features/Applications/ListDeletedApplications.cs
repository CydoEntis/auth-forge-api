using AuthForge.Api.Common;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record DeletedApplicationListItem(
    Guid Id,
    string Name,
    string Slug,
    int UserCount,
    DateTime DeletedAtUtc,
    int DaysUntilPermanentDeletion
);

public sealed record ListDeletedApplicationsResponse(
    List<DeletedApplicationListItem> Applications
);

public class ListDeletedApplicationsHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<ListDeletedApplicationsHandler> _logger;

    public ListDeletedApplicationsHandler(AppDbContext context, ILogger<ListDeletedApplicationsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ListDeletedApplicationsResponse> HandleAsync(CancellationToken ct)
    {
        var applications = await _context.Applications
            .IgnoreQueryFilters()
            .Where(a => a.IsDeleted)
            .Select(a => new DeletedApplicationListItem(
                a.Id,
                a.Name,
                a.Slug,
                a.Users.Count,
                a.DeletedAtUtc!.Value,
                30 - (int)(DateTime.UtcNow - a.DeletedAtUtc!.Value).TotalDays
            ))
            .OrderByDescending(a => a.DeletedAtUtc)
            .ToListAsync(ct);

        return new ListDeletedApplicationsResponse(applications);
    }
}

public static class ListDeletedApplications
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}/applications/deleted", async (
                ListDeletedApplicationsHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(ct);
                return Results.Ok(ApiResponse<ListDeletedApplicationsResponse>.Ok(response));
            })
            .WithName("ListDeletedApplications")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}