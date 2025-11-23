using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record LockUserRequest(
    int LockoutDurationMinutes = 60
);

public sealed record LockUserResponse(
    Guid Id,
    string Email,
    DateTime LockedOutUntil,
    string Message
);

public sealed class LockUserValidator : AbstractValidator<LockUserRequest>
{
    public LockUserValidator()
    {
        RuleFor(x => x.LockoutDurationMinutes)
            .GreaterThan(0).WithMessage("Lockout duration must be greater than 0")
            .LessThanOrEqualTo(43200).WithMessage("Maximum lockout duration is 30 days (43200 minutes)");
    }
}

public sealed class LockUserHandler
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<LockUserHandler> _logger;

    public LockUserHandler(
        AppDbContext context,
        ICurrentUserService currentUser,
        ILogger<LockUserHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<LockUserResponse> HandleAsync(
        Guid applicationId,
        Guid userId,
        LockUserRequest request,
        CancellationToken ct)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == applicationId, ct);

        if (application == null)
        {
            throw new NotFoundException($"Application {applicationId} not found");
        }

        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId && u.ApplicationId == applicationId, ct);

        if (user == null)
        {
            throw new NotFoundException($"User {userId} not found in application {applicationId}");
        }

        var lockedUntil = DateTime.UtcNow.AddMinutes(request.LockoutDurationMinutes);
        user.LockedOutUntil = lockedUntil;
        user.UpdatedAtUtc = DateTime.UtcNow;

        foreach (var token in user.RefreshTokens.Where(rt => !rt.IsRevoked))
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Admin {AdminId} LOCKED user {UserId} ({Email}) in application {AppId} until {LockedUntil}",
            _currentUser.UserId,
            userId,
            user.Email,
            applicationId,
            lockedUntil);

        return new LockUserResponse(
            userId,
            user.Email,
            lockedUntil,
            $"User {user.Email} has been locked out until {lockedUntil:yyyy-MM-dd HH:mm:ss} UTC"
        );
    }
}

public static class LockUser
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/applications/{{appId:guid}}/users/{{userId:guid}}/lock", async (
                Guid appId,
                Guid userId,
                LockUserRequest request,
                LockUserHandler handler,
                CancellationToken ct) =>
            {
                var validator = new LockUserValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(appId, userId, request, ct);
                return Results.Ok(ApiResponse<LockUserResponse>.Ok(response));
            })
            .WithName("LockUser")
            .WithTags("User Management")
            .RequireAuthorization("Admin");
    }
}