using AuthForge.Api.Common;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Admin;

public sealed record AdminVerifyPasswordResetTokenRequest(string Token);

public sealed record AdminVerifyPasswordResetTokenResponse(bool IsValid, string? Message);

public sealed class AdminVerifyPasswordResetTokenValidator : AbstractValidator<AdminVerifyPasswordResetTokenRequest>
{
    public AdminVerifyPasswordResetTokenValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Reset token is required");
    }
}

public class AdminVerifyPasswordResetTokenHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminVerifyPasswordResetTokenHandler> _logger;

    public AdminVerifyPasswordResetTokenHandler(
        AppDbContext context,
        ILogger<AdminVerifyPasswordResetTokenHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AdminVerifyPasswordResetTokenResponse> HandleAsync(
        AdminVerifyPasswordResetTokenRequest request,
        CancellationToken ct)
    {
        var resetToken = await _context.AdminPasswordResetTokens
            .Include(t => t.Admin)
            .FirstOrDefaultAsync(t => t.Token == request.Token, ct);

        if (resetToken == null)
        {
            _logger.LogWarning("Invalid reset token attempted: {Token}",
                request.Token?.Substring(0, Math.Min(10, request.Token?.Length ?? 0)));
            return new AdminVerifyPasswordResetTokenResponse(false, "Invalid reset token.");
        }

        if (resetToken.IsUsed)
        {
            _logger.LogWarning("Already used reset token attempted: {TokenId}", resetToken.Id);
            return new AdminVerifyPasswordResetTokenResponse(false, "This reset link has already been used.");
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired reset token attempted: {TokenId}", resetToken.Id);
            return new AdminVerifyPasswordResetTokenResponse(false,
                "This reset link has expired. Please request a new one.");
        }

        _logger.LogInformation("Valid reset token verified for admin: {AdminId}", resetToken.AdminId);
        return new AdminVerifyPasswordResetTokenResponse(true, "Token is valid.");
    }
}

public static class AdminVerifyPasswordResetToken
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/admin/verify-password-reset-token", async (
                AdminVerifyPasswordResetTokenRequest request,
                AdminVerifyPasswordResetTokenHandler handler,
                CancellationToken ct) =>
            {
                var validator = new AdminVerifyPasswordResetTokenValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);

                if (!response.IsValid)
                {
                    return Results.BadRequest(ApiResponse<AdminVerifyPasswordResetTokenResponse>.Fail(
                        ErrorCodes.InvalidToken,
                        response.Message!
                    ));
                }

                return Results.Ok(ApiResponse<AdminVerifyPasswordResetTokenResponse>.Ok(response));
            })
            .WithName("AdminVerifyResetToken")
            .WithTags("Admin")
            .AllowAnonymous();
    }
}