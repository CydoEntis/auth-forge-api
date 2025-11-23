using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace AuthForge.Api.Features.Users;

public sealed record UserForgotPasswordRequest(string Email);

public sealed record UserForgotPasswordResponse(string Message);

public sealed class UserForgotPasswordValidator : AbstractValidator<UserForgotPasswordRequest>
{
    public UserForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}

public sealed class UserForgotPasswordHandler
{
    private readonly AppDbContext _context;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IEmailServiceFactory _emailServiceFactory;
    private readonly ILogger<UserForgotPasswordHandler> _logger;

    public UserForgotPasswordHandler(
        AppDbContext context,
        IEmailTemplateService emailTemplateService,
        IEmailServiceFactory emailServiceFactory,
        ILogger<UserForgotPasswordHandler> logger)
    {
        _context = context;
        _emailTemplateService = emailTemplateService;
        _emailServiceFactory = emailServiceFactory;
        _logger = logger;
    }

    public async Task<UserForgotPasswordResponse> HandleAsync(
        Guid applicationId,
        UserForgotPasswordRequest request,
        CancellationToken ct)
    {
        var app = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == applicationId && !a.IsDeleted, ct);

        if (app == null || !app.IsActive)
        {
            _logger.LogWarning(
                "Forgot password attempt for invalid application: {AppId}",
                applicationId);
            throw new BadRequestException("Application not found or inactive");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.ApplicationId == applicationId
                && u.Email == request.Email, ct);

        if (user == null)
        {
            _logger.LogWarning(
                "Password reset requested for non-existent email {Email} in app {AppId}",
                request.Email,
                applicationId);
            return new UserForgotPasswordResponse(
                "If an account exists with that email, a password reset link has been sent.");
        }

        var resetToken = GenerateResetToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        var existingToken = await _context.UserPasswordResetTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id && !t.IsUsed, ct);

        if (existingToken != null)
        {
            existingToken.Token = resetToken;
            existingToken.ExpiresAt = expiresAt;
            existingToken.CreatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            var passwordResetToken = new Entities.UserPasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = resetToken,
                ExpiresAt = expiresAt,
                IsUsed = false,
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.UserPasswordResetTokens.Add(passwordResetToken);
        }

        await _context.SaveChangesAsync(ct);

        try
        {
            await SendResetEmailAsync(user, app, resetToken, ct);
            _logger.LogInformation(
                "Password reset email sent to {Email}",
                user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
            throw new InvalidOperationException("Failed to send password reset email");
        }

        return new UserForgotPasswordResponse(
            "If an account exists with that email, a password reset link has been sent.");
    }

    private static string GenerateResetToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    private async Task SendResetEmailAsync(
        Entities.User user,
        Entities.Application app,
        string resetToken,
        CancellationToken ct)
    {
        var resetUrl = $"{app.PasswordResetCallbackUrl}?token={resetToken}";

        var emailMessage = await _emailTemplateService.CreatePasswordResetEmailAsync(
            toEmail: user.Email,
            toName: user.FirstName ?? "User",
            resetUrl: resetUrl,
            appName: app.Name
        );

        string fromAddress;
        string? fromName;

        if (app.UseGlobalEmailSettings)
        {
            fromAddress = await _emailServiceFactory.GetFromAddressAsync(ct);
            fromName = await _emailServiceFactory.GetFromNameAsync(ct);
        }
        else
        {
            fromAddress = app.FromEmail ?? throw new InvalidOperationException("Application email not configured");
            fromName = app.FromName;
        }

        var finalMessage = emailMessage with
        {
            From = fromAddress,
            FromName = fromName ?? app.Name
        };

        var emailService = await _emailServiceFactory.CreateAsync(ct);
        var result = await emailService.SendAsync(finalMessage, ct);

        if (!result.Success)
        {
            _logger.LogError("Failed to send password reset email: {Error}", result.ErrorMessage);
            throw new InvalidOperationException("Failed to send password reset email");
        }
    }
}

public static class UserForgotPassword
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/apps/{{appId:guid}}/auth/forgot-password", async (
                Guid appId,
                UserForgotPasswordRequest request,
                UserForgotPasswordHandler handler,
                CancellationToken ct) =>
            {
                var validator = new UserForgotPasswordValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(appId, request, ct);
                return Results.Ok(ApiResponse<UserForgotPasswordResponse>.Ok(response));
            })
            .WithName("UserForgotPassword")
            .WithTags("User Auth")
            .AllowAnonymous();
    }
}