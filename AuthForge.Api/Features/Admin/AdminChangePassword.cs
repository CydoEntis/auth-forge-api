using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Setup;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Admin;

public sealed record AdminChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

public sealed record AdminChangePasswordResponse(string Message);

public sealed class AdminChangePasswordRequestValidator
    : AbstractValidator<AdminChangePasswordRequest>
{
    public AdminChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required")
            .MinimumLength(8)
            .WithMessage("New password must be at least 8 characters")
            .Matches(@"[A-Z]")
            .WithMessage("New password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
            .WithMessage("New password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
            .WithMessage("New password must contain at least one number")
            .Matches(@"[^a-zA-Z0-9]")
            .WithMessage("New password must contain at least one special character");

        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("New password must be different from current password");
    }
}

public class AdminChangePasswordHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminChangePasswordHandler> _logger;
    private readonly PasswordHasher<Entities.Admin> _passwordHasher;
    private readonly ICurrentUserService _currentUserService;

    public AdminChangePasswordHandler(AppDbContext context, ILogger<AdminChangePasswordHandler> logger,
        PasswordHasher<Entities.Admin> passwordHasher, ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
    }

    public async Task<AdminChangePasswordResponse> HandleAsync(AdminChangePasswordRequest request, CancellationToken ct)
    {
        var currentUserId = _currentUserService.UserId;
        if (!Guid.TryParse(currentUserId, out var userGuid))
        {
            _logger.LogInformation("Password change attempt for admin with {currentUserId} failed", currentUserId);
            throw new InvalidOperationException("Invalid user ID in token.");
        }

        var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == userGuid, ct);
        if (admin is null)
        {
            _logger.LogInformation("Admin with {currentUserId} does not exist", currentUserId);
            throw new NotFoundException($"No admin with {currentUserId} could be found");
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(null!, admin.PasswordHash, request.CurrentPassword);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Failed password verification for admin {AdminId}", userGuid);
            throw new UnauthorizedException("Current password is incorrect.");
        }

        var hashedPassword = _passwordHasher.HashPassword(null!, request.NewPassword);
        admin.PasswordHash = hashedPassword;
        admin.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Admin {AdminId} changed password successfully", userGuid);
        return new AdminChangePasswordResponse("Password changed successfully.");
    }
}

public static class AdminChangePassword
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/admin/change-password", async (
                AdminChangePasswordRequest request,
                AdminChangePasswordHandler handler,
                CancellationToken ct) =>
            {
                var validator = new AdminChangePasswordRequestValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<AdminChangePasswordResponse>.Ok(response));
            })
            .WithName("AdminChangePassword")
            .WithTags("Admin")
            .AllowAnonymous()
            .RequireAuthorization("Admin");
    }
}