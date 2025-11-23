using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record SoftDeleteApplicationResponse(
    Guid Id,
    string Name,
    int UserCount,
    DateTime DeletedAtUtc,
    string Message
);

public class SoftDeleteApplicationHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<SoftDeleteApplicationHandler> _logger;

    public SoftDeleteApplicationHandler(AppDbContext context, ILogger<SoftDeleteApplicationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SoftDeleteApplicationResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var application = await _context.Applications
            .Include(a => a.Users)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (application == null)
        {
            throw new NotFoundException($"Application with ID {id} not found");
        }

        if (application.IsDeleted)
        {
            throw new ConflictException("Application is already deleted");
        }

        var userCount = application.Users.Count;
        var deletedAt = DateTime.UtcNow;

        application.IsDeleted = true;
        application.DeletedAtUtc = deletedAt;
        application.IsActive = false;

        await _context.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Application soft deleted: {Name} ({Id}) with {UserCount} users",
            application.Name, id, userCount);

        return new SoftDeleteApplicationResponse(
            id,
            application.Name,
            userCount,
            deletedAt,
            $"Application '{application.Name}' has been deactivated. " +
            "It can be restored at any time or permanently deleted manually."
        );
    }
}

public static class SoftDeleteApplication
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapDelete($"{prefix}/applications/{{id:guid}}", async (
                Guid id,
                [FromServices] SoftDeleteApplicationHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(id, ct);
                return Results.Ok(ApiResponse<SoftDeleteApplicationResponse>.Ok(response));
            })
            .WithName("SoftDeleteApplication")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}