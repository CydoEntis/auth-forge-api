using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Admin;

public sealed record GetAdminResponse(
    Guid Id,
    string Email,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public class GetAdminHandler
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetAdminHandler> _logger;

    public GetAdminHandler(
        AppDbContext context,
        ICurrentUserService currentUserService,
        ILogger<GetAdminHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<GetAdminResponse> HandleAsync(CancellationToken ct)
    {
        var currentUserId = _currentUserService.UserId;
        
        if (!Guid.TryParse(currentUserId, out var adminId))
        {
            throw new UnauthorizedException("Invalid user ID in token.");
        }

        var admin = await _context.Admins
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == adminId, ct);

        if (admin == null)
        {
            _logger.LogWarning("Admin not found: {AdminId}", adminId);
            throw new NotFoundException("Admin not found.");
        }

        return new GetAdminResponse(
            Id: admin.Id,
            Email: admin.Email,
            CreatedAtUtc: admin.CreatedAtUtc,
            UpdatedAtUtc: admin.UpdatedAtUtc
        );
    }
}

public static class GetAdmin
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}/admin/me", async (
                [FromServices] GetAdminHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(ct);
                return Results.Ok(ApiResponse<GetAdminResponse>.Ok(response));
            })
            .WithName("GetGetAdmin")
            .WithTags("Admin")
            .RequireAuthorization("Admin"); 
    }
}