using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record RestoreApplicationResponse(
    Guid Id,
    string Name,
    DateTime RestoredAtUtc,
    string Message
);

public class RestoreApplicationHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<RestoreApplicationHandler> _logger;

    public RestoreApplicationHandler(AppDbContext context, ILogger<RestoreApplicationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RestoreApplicationResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var application = await _context.Applications
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted, ct);

        if (application == null)
        {
            throw new NotFoundException($"Deleted application with ID {id} not found");
        }

        var restoredAt = DateTime.UtcNow;

        application.IsDeleted = false;
        application.DeletedAtUtc = null;
        application.IsActive = true;
        application.UpdatedAtUtc = restoredAt;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Application restored: {Name} ({Id})", application.Name, id);

        return new RestoreApplicationResponse(
            id,
            application.Name,
            restoredAt,
            $"Application '{application.Name}' has been successfully restored"
        );
    }
}

public static class RestoreApplicationFeature
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/applications/{{id:guid}}/restore", async (
                Guid id,
                [FromServices] RestoreApplicationHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(id, ct);
                return Results.Ok(ApiResponse<RestoreApplicationResponse>.Ok(response));
            })
            .WithName("RestoreApplication")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}