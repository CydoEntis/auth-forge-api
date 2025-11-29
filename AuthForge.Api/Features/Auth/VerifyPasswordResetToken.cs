using AuthForge.Api.Common;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Auth;

public sealed record VerifyPasswordResetTokenRequest(string Token);

public sealed record VerifyPasswordResetTokenResponse(bool IsValid, string? Message);

public sealed class VerifyPasswordResetTokenValidator : AbstractValidator<VerifyPasswordResetTokenRequest>
{
    public VerifyPasswordResetTokenValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Reset token is required");
    }
}

public class VerifyPasswordResetTokenHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<VerifyPasswordResetTokenHandler> _logger;

    public VerifyPasswordResetTokenHandler(
        AppDbContext context,
        ILogger<VerifyPasswordResetTokenHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<VerifyPasswordResetTokenResponse> HandleAsync(
        VerifyPasswordResetTokenRequest request,
        CancellationToken ct)
    {
        var resetToken = await _context.AdminPasswordResetTokens
            .Include(t => t.Admin)
            .FirstOrDefaultAsync(t => t.Token == request.Token, ct);

        if (resetToken == null)
        {
            _logger.LogWarning("Invalid reset token attempted: {Token}",
                request.Token?.Substring(0, Math.Min(10, request.Token?.Length ?? 0)));
            return new VerifyPasswordResetTokenResponse(false, "Invalid reset token.");
        }

        if (resetToken.IsUsed)
        {
            _logger.LogWarning("Already used reset token attempted: {TokenId}", resetToken.Id);
            return new VerifyPasswordResetTokenResponse(false, "This reset link has already been used.");
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired reset token attempted: {TokenId}", resetToken.Id);
            return new VerifyPasswordResetTokenResponse(false,
                "This reset link has expired. Please request a new one.");
        }

        _logger.LogInformation("Valid reset token verified for account: {AdminId}", resetToken.AdminId);
        return new VerifyPasswordResetTokenResponse(true, "Token is valid.");
    }
}

public static class VerifyPasswordResetToken
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/verify-password-reset-token", async (
                VerifyPasswordResetTokenRequest request,
                VerifyPasswordResetTokenHandler handler,
                CancellationToken ct) =>
            {
                var validator = new VerifyPasswordResetTokenValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);

                if (!response.IsValid)
                {
                    return Results.BadRequest(ApiResponse<VerifyPasswordResetTokenResponse>.Fail(
                        ErrorCodes.InvalidToken,
                        response.Message!
                    ));
                }

                return Results.Ok(ApiResponse<VerifyPasswordResetTokenResponse>.Ok(response));
            })
            .WithName("VerifyResetToken")
            .WithTags("Auth")
            .AllowAnonymous();
    }
}