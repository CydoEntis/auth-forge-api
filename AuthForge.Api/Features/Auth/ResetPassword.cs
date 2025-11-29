using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Auth;

public sealed record ResetPasswordRequest(
    string Token,
    string NewPassword,
    string ConfirmNewPassword
);

public sealed record ResetPasswordResponse(string Message);

public sealed class ResetPasswordValidator
    : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Reset token is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Must contain uppercase")
            .Matches(@"[a-z]").WithMessage("Must contain lowercase")
            .Matches(@"[0-9]").WithMessage("Must contain a number")
            .Matches(@"[\W_]").WithMessage("Must contain special character");

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords must match");
    }
}

public class ResetPasswordHandler
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Entities.Admin> _passwordHasher;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(
        AppDbContext context,
        PasswordHasher<Entities.Admin> passwordHasher,
        ILogger<ResetPasswordHandler> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<ResetPasswordResponse> HandleAsync(
        ResetPasswordRequest request,
        CancellationToken ct)
    {
        var resetToken = await _context.AdminPasswordResetTokens
            .Include(t => t.Admin)
            .FirstOrDefaultAsync(t => t.Token == request.Token, ct);

        if (resetToken == null)
        {
            _logger.LogWarning("Invalid password reset token attempted");
            throw new BadRequestException("Invalid or expired reset token.");
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired password reset token used: {TokenId}", resetToken.Id);
            throw new BadRequestException("Reset token has expired. Please request a new one.");
        }

        if (resetToken.IsUsed)
        {
            _logger.LogWarning("Already used password reset token: {TokenId}", resetToken.Id);
            throw new BadRequestException("Reset token has already been used.");
        }

        var admin = resetToken.Admin;
        admin.PasswordHash = _passwordHasher.HashPassword(null!, request.NewPassword);
        admin.UpdatedAtUtc = DateTime.UtcNow;

        resetToken.IsUsed = true;

        var refreshTokens = await _context.AdminRefreshTokens
            .Where(rt => rt.AdminId == admin.Id && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Admin password reset completed: {AdminId}", admin.Id);

        return new ResetPasswordResponse(
            "Password reset successful. Please log in with your new password.");
    }
}

public static class ResetPassword
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/reset-password", async (
                ResetPasswordRequest request,
                ResetPasswordHandler handler,
                CancellationToken ct) =>
            {
                var validator = new ResetPasswordValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<ResetPasswordResponse>.Ok(response));
            })
            .WithName("ResetPassword")
            .WithTags("Auth")
            .AllowAnonymous();
    }
}