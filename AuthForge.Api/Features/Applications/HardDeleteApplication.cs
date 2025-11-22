using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record HardDeleteApplicationResponse(
    Guid Id,
    string Name,
    int DeletedUserCount,
    string Message
);

public class HardDeleteApplicationHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<HardDeleteApplicationHandler> _logger;

    public HardDeleteApplicationHandler(AppDbContext context, ILogger<HardDeleteApplicationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HardDeleteApplicationResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var application = await _context.Applications
            .IgnoreQueryFilters()
            .Include(a => a.Users)
            .FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted, ct);

        if (application == null)
        {
            throw new NotFoundException($"Deleted application with ID {id} not found");
        }

        var userCount = application.Users.Count;
        var appName = application.Name;

        _context.Applications.Remove(application);
        await _context.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Application PERMANENTLY deleted: {Name} ({Id}) with {UserCount} users",
            appName, id, userCount);

        return new HardDeleteApplicationResponse(
            id,
            appName,
            userCount,
            $"Application '{appName}' and {userCount} users have been PERMANENTLY deleted and cannot be recovered"
        );
    }
}

public static class HardDeleteApplication
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapDelete($"{prefix}/applications/{{id:guid}}/permanent", async (
                Guid id,
                HardDeleteApplicationHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(id, ct);
                return Results.Ok(ApiResponse<HardDeleteApplicationResponse>.Ok(response));
            })
            .WithName("HardDeleteApplication")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}