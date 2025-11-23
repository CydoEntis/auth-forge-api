using AuthForge.Api.Common;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Users;

public sealed record UserVerifyPasswordResetTokenRequest(string Token);

public sealed record UserVerifyPasswordResetTokenResponse(
    bool Valid,
    string Message
);

public sealed class UserVerifyPasswordResetTokenValidator : AbstractValidator<UserVerifyPasswordResetTokenRequest>
{
    public UserVerifyPasswordResetTokenValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required");
    }
}

public sealed class UserVerifyPasswordResetTokenHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserVerifyPasswordResetTokenHandler> _logger;

    public UserVerifyPasswordResetTokenHandler(
        AppDbContext context,
        ILogger<UserVerifyPasswordResetTokenHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserVerifyPasswordResetTokenResponse> HandleAsync(
        Guid applicationId,
        UserVerifyPasswordResetTokenRequest request,
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
                "Invalid password reset token verification attempt for app {AppId}",
                applicationId);
            return new UserVerifyPasswordResetTokenResponse(
                Valid: false,
                Message: "Invalid or expired reset token"
            );
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning(
                "Expired password reset token verified for user {UserId}. Expired at {ExpiredAt}",
                resetToken.UserId,
                resetToken.ExpiresAt);
            return new UserVerifyPasswordResetTokenResponse(
                Valid: false,
                Message: "Reset token has expired. Please request a new one."
            );
        }

        if (resetToken.IsUsed)
        {
            _logger.LogWarning(
                "Already used password reset token verified for user {UserId}",
                resetToken.UserId);
            return new UserVerifyPasswordResetTokenResponse(
                Valid: false,
                Message: "Reset token has already been used"
            );
        }

        _logger.LogInformation(
            "Valid password reset token verified for user {UserId}",
            resetToken.UserId);

        return new UserVerifyPasswordResetTokenResponse(
            Valid: true,
            Message: "Token is valid"
        );
    }
}

public static class UserVerifyPasswordResetToken
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/apps/{{appId:guid}}/auth/verify-reset-token", async (
                Guid appId,
                UserVerifyPasswordResetTokenRequest request,
                UserVerifyPasswordResetTokenHandler handler,
                CancellationToken ct) =>
            {
                var validator = new UserVerifyPasswordResetTokenValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(appId, request, ct);
                return Results.Ok(ApiResponse<UserVerifyPasswordResetTokenResponse>.Ok(response));
            })
            .WithName("UserVerifyPasswordResetToken")
            .WithTags("User Auth")
            .AllowAnonymous();
    }
}