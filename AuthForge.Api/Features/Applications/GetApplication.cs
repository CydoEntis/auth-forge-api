using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record GetApplicationResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string ClientId,
    bool IsActive,
    int TotalUsers,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public class GetApplicationHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<GetApplicationHandler> _logger;

    public GetApplicationHandler(AppDbContext context, ILogger<GetApplicationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetApplicationResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var application = await _context.Applications
            .Where(a => a.Id == id)
            .Select(a => new GetApplicationResponse(
                a.Id,
                a.Name,
                a.Slug,
                a.Description,
                a.ClientId,
                a.IsActive,
                a.Users.Count,
                a.CreatedAtUtc,
                a.UpdatedAtUtc
            ))
            .FirstOrDefaultAsync(ct);

        if (application == null)
        {
            _logger.LogWarning("Application not found: {Id}", id);
            throw new NotFoundException($"Application with ID {id} not found");
        }

        _logger.LogInformation("Retrieved application: {Name} ({Id})", application.Name, application.Id);

        return application;
    }
}

public static class GetApplication
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}/applications/{{id:guid}}", async (
                Guid id,
                [FromServices] GetApplicationHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(id, ct);
                return Results.Ok(ApiResponse<GetApplicationResponse>.Ok(response));
            })
            .WithName("GetApplication")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}