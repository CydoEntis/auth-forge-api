using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Users;

public sealed record UserResetPasswordRequest(
    string Token,
    string NewPassword,
    string ConfirmNewPassword
);

public sealed record UserResetPasswordResponse(string Message);

public sealed class UserResetPasswordValidator : AbstractValidator<UserResetPasswordRequest>
{
    public UserResetPasswordValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords must match");
    }
}

public sealed class UserResetPasswordHandler
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Entities.User> _passwordHasher;
    private readonly ILogger<UserResetPasswordHandler> _logger;

    public UserResetPasswordHandler(
        AppDbContext context,
        PasswordHasher<Entities.User> passwordHasher,
        ILogger<UserResetPasswordHandler> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<UserResetPasswordResponse> HandleAsync(
        Guid applicationId,
        UserResetPasswordRequest request,
        CancellationToken ct)
    {
        var resetToken = await _context.UserPasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == request.Token
                && t.User.ApplicationId == applicationId, ct);

        if (resetToken == null)
        {
            _logger.LogWarning(
                "Invalid password reset token attempted for app {AppId}",
                applicationId);
            throw new BadRequestException("Invalid or expired reset token");
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning(
                "Expired password reset token used for user {UserId}. Expired at {ExpiredAt}",
                resetToken.UserId,
                resetToken.ExpiresAt);
            throw new BadRequestException("Reset token has expired. Please request a new one.");
        }

        if (resetToken.IsUsed)
        {
            _logger.LogWarning(
                "Already used password reset token for user {UserId}",
                resetToken.UserId);
            throw new BadRequestException("Reset token has already been used");
        }

        var user = resetToken.User;

        user.PasswordHash = _passwordHasher.HashPassword(null!, request.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;

        user.FailedLoginAttempts = 0;
        user.LockedOutUntil = null;

        resetToken.IsUsed = true;

        var refreshTokens = await _context.UserRefreshTokens
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "User {UserId} successfully reset password. {TokenCount} refresh tokens revoked.",
            user.Id,
            refreshTokens.Count);

        return new UserResetPasswordResponse(
            "Password reset successful. Please log in with your new password.");
    }
}

public static class UserResetPassword
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/apps/{{appId:guid}}/auth/reset-password", async (
                Guid appId,
                UserResetPasswordRequest request,
                UserResetPasswordHandler handler,
                CancellationToken ct) =>
            {
                var validator = new UserResetPasswordValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(appId, request, ct);
                return Results.Ok(ApiResponse<UserResetPasswordResponse>.Ok(response));
            })
            .WithName("UserResetPassword")
            .WithTags("User Auth")
            .AllowAnonymous();
    }
}