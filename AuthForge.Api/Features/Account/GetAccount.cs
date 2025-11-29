using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Account;

public sealed record GetAccountResponse(
    Guid Id,
    string Email,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public class GetAccountHandler
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetAccountHandler> _logger;

    public GetAccountHandler(
        AppDbContext context,
        ICurrentUserService currentUserService,
        ILogger<GetAccountHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<GetAccountResponse> HandleAsync(CancellationToken ct)
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
            _logger.LogWarning("Account not found: {AdminId}", adminId);
            throw new NotFoundException("Account not found.");
        }

        return new GetAccountResponse(
            Id: admin.Id,
            Email: admin.Email,
            CreatedAtUtc: admin.CreatedAtUtc,
            UpdatedAtUtc: admin.UpdatedAtUtc
        );
    }
}

public static class GetAccount
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}/me", async (
                [FromServices] GetAccountHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(ct);
                return Results.Ok(ApiResponse<GetAccountResponse>.Ok(response));
            })
            .WithName("GetAccount")
            .WithTags("Account")
            .RequireAuthorization("Admin");
    }
}