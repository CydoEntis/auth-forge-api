using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Users.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Users;

public sealed record GetCurrentUserResponse(UserDto User);

public sealed class GetCurrentUserHandler
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetCurrentUserHandler> _logger;

    public GetCurrentUserHandler(
        AppDbContext context,
        ICurrentUserService currentUserService,
        ILogger<GetCurrentUserHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<GetCurrentUserResponse> HandleAsync(
        Guid applicationId,
        CancellationToken ct)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            _logger.LogWarning("GetCurrentUser called without valid user ID in token");
            throw new UnauthorizedException("Invalid or missing authentication token");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Id == userGuid
                && u.ApplicationId == applicationId, ct);

        if (user == null)
        {
            _logger.LogWarning(
                "User {UserId} not found for application {AppId}",
                userGuid,
                applicationId);
            throw new NotFoundException("User not found");
        }

        _logger.LogInformation(
            "User {UserId} retrieved their profile for app {AppId}",
            user.Id,
            applicationId);

        return new GetCurrentUserResponse(User: MapToDto(user));
    }

    private static UserDto MapToDto(Entities.User user) =>
        new(
            Id: user.Id,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            EmailVerified: user.EmailVerified,
            LastLoginAtUtc: user.LastLoginAtUtc,
            CreatedAtUtc: user.CreatedAtUtc
        );
}

public static class GetCurrentUser
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}/apps/{{appId:guid}}/auth/me", async (
                Guid appId,
                GetCurrentUserHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(appId, ct);
                return Results.Ok(ApiResponse<GetCurrentUserResponse>.Ok(response));
            })
            .WithName("GetCurrentUser")
            .WithTags("User Auth")
            .RequireAuthorization();
    }
}