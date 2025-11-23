using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Users;

public sealed record UserChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);

public sealed record UserChangePasswordResponse(string Message);

public sealed class UserChangePasswordValidator : AbstractValidator<UserChangePasswordRequest>
{
    public UserChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("New password must be different from current password");

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords must match");
    }
}

public sealed class UserChangePasswordHandler
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly PasswordHasher<Entities.User> _passwordHasher;
    private readonly ILogger<UserChangePasswordHandler> _logger;

    public UserChangePasswordHandler(
        AppDbContext context,
        ICurrentUserService currentUserService,
        PasswordHasher<Entities.User> passwordHasher,
        ILogger<UserChangePasswordHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<UserChangePasswordResponse> HandleAsync(
        Guid applicationId,
        UserChangePasswordRequest request,
        CancellationToken ct)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            _logger.LogWarning("Change password attempt without valid user ID in token");
            throw new UnauthorizedException("Invalid or missing authentication token");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Id == userGuid
                && u.ApplicationId == applicationId, ct);

        if (user == null)
        {
            _logger.LogWarning(
                "User {UserId} not found for app {AppId} during password change",
                userGuid,
                applicationId);
            throw new NotFoundException("User not found");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            null!,
            user.PasswordHash,
            request.CurrentPassword);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning(
                "Failed password verification during change for user {UserId}",
                user.Id);
            throw new UnauthorizedException("Current password is incorrect");
        }

        user.PasswordHash = _passwordHasher.HashPassword(null!, request.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;

        var refreshTokens = await _context.UserRefreshTokens
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "User {UserId} changed password. {TokenCount} refresh tokens revoked.",
            user.Id,
            refreshTokens.Count);

        return new UserChangePasswordResponse(
            "Password changed successfully. Please log in again with your new password.");
    }
}

public static class UserChangePassword
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/apps/{{appId:guid}}/auth/change-password", async (
                Guid appId,
                UserChangePasswordRequest request,
                UserChangePasswordHandler handler,
                CancellationToken ct) =>
            {
                var validator = new UserChangePasswordValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(appId, request, ct);
                return Results.Ok(ApiResponse<UserChangePasswordResponse>.Ok(response));
            })
            .WithName("UserChangePassword")
            .WithTags("User Auth")
            .RequireAuthorization();
    }
}