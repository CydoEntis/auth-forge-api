using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Users;

public sealed record UserVerifyEmailRequest(string Token);

public sealed record UserVerifyEmailResponse(string Message);

public sealed class UserVerifyEmailValidator : AbstractValidator<UserVerifyEmailRequest>
{
    public UserVerifyEmailValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Verification token is required");
    }
}

public sealed class UserVerifyEmailHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserVerifyEmailHandler> _logger;

    public UserVerifyEmailHandler(
        AppDbContext context,
        ILogger<UserVerifyEmailHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserVerifyEmailResponse> HandleAsync(
        Guid applicationId,
        UserVerifyEmailRequest request,
        CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.ApplicationId == applicationId
                && u.EmailVerificationToken == request.Token, ct);

        if (user == null)
        {
            _logger.LogWarning(
                "Invalid email verification token attempted for app {AppId}",
                applicationId);
            throw new BadRequestException("Invalid or expired verification token");
        }

        if (user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning(
                "Expired email verification token for user {UserId}. Expired at {ExpiredAt}",
                user.Id,
                user.EmailVerificationTokenExpiresAt);
            throw new BadRequestException("Verification token has expired. Please request a new one.");
        }

        if (user.EmailVerified)
        {
            _logger.LogInformation(
                "User {UserId} attempted to verify already verified email",
                user.Id);
            return new UserVerifyEmailResponse("Email already verified");
        }

        user.EmailVerified = true;
        user.EmailVerificationToken = null; 
        user.EmailVerificationTokenExpiresAt = null;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "User {UserId} successfully verified email {Email}",
            user.Id,
            user.Email);

        return new UserVerifyEmailResponse("Email verified successfully. You can now log in.");
    }
}

public static class UserVerifyEmail
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/apps/{{appId:guid}}/auth/verify-email", async (
                Guid appId,
                UserVerifyEmailRequest request,
                UserVerifyEmailHandler handler,
                CancellationToken ct) =>
            {
                var validator = new UserVerifyEmailValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(appId, request, ct);
                return Results.Ok(ApiResponse<UserVerifyEmailResponse>.Ok(response));
            })
            .WithName("UserVerifyEmail")
            .WithTags("User Auth")
            .AllowAnonymous(); 
    }
}