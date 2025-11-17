using System.Security.Cryptography;
using AuthForge.Api.Common;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Admin;

public sealed record AdminForgotPasswordRequest(string Email);
public sealed record AdminForgotPasswordResponse(string Message);

public sealed class AdminForgotPasswordValidator : AbstractValidator<AdminForgotPasswordRequest>
{
    public AdminForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}

public class AdminForgotPasswordHandler
{
    private readonly AppDbContext _context;
    private readonly ConfigDbContext _configDb;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IEmailServiceFactory _emailServiceFactory;
    private readonly ILogger<AdminForgotPasswordHandler> _logger;

    public AdminForgotPasswordHandler(
        AppDbContext context,
        ConfigDbContext configDb,
        IEmailTemplateService emailTemplateService,
        IEmailServiceFactory emailServiceFactory,
        ILogger<AdminForgotPasswordHandler> logger)
    {
        _context = context;
        _configDb = configDb;
        _emailTemplateService = emailTemplateService;
        _emailServiceFactory = emailServiceFactory;
        _logger = logger;
    }

    public async Task<AdminForgotPasswordResponse> HandleAsync(
        AdminForgotPasswordRequest request,
        CancellationToken ct)
    {
        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Email == request.Email, ct);

        if (admin == null)
        {
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
            return new AdminForgotPasswordResponse(
                "If an account exists with that email, a password reset link has been sent.");
        }

        var resetToken = GenerateResetToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        var existingToken = await _context.AdminPasswordResetTokens
            .FirstOrDefaultAsync(t => t.AdminId == admin.Id && !t.IsUsed, ct);

        if (existingToken != null)
        {
            existingToken.Token = resetToken;
            existingToken.ExpiresAt = expiresAt;
            existingToken.CreatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            var passwordResetToken = new Entities.AdminPasswordResetToken
            {
                Id = Guid.NewGuid(),
                AdminId = admin.Id,
                Token = resetToken,
                ExpiresAt = expiresAt,
                IsUsed = false,
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.AdminPasswordResetTokens.Add(passwordResetToken);
        }

        await _context.SaveChangesAsync(ct);

        await SendResetEmailAsync(admin.Email, resetToken, ct);

        _logger.LogInformation("Password reset token generated for admin: {Email}", admin.Email);

        return new AdminForgotPasswordResponse(
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

    private async Task SendResetEmailAsync(string email, string resetToken, CancellationToken ct)
    {
        var config = await _configDb.Configuration.FirstOrDefaultAsync(ct);

        if (config?.AuthForgeDomain == null)
        {
            _logger.LogError("AuthForge domain not configured");
            throw new InvalidOperationException("AuthForge domain not configured");
        }

        var resetUrl = $"{config.AuthForgeDomain}/admin/reset-password?token={resetToken}";

        var emailMessage = await _emailTemplateService.CreatePasswordResetEmailAsync(
            toEmail: email,
            toName: "Admin",
            resetUrl: resetUrl,  
            appName: "AuthForge"
        );

        var fromAddress = await _emailServiceFactory.GetFromAddressAsync(ct);
        var fromName = await _emailServiceFactory.GetFromNameAsync(ct);

        var finalMessage = emailMessage with 
        { 
            From = fromAddress,
            FromName = fromName ?? "AuthForge"
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

public static class AdminForgotPassword
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/admin/forgot-password", async (
                AdminForgotPasswordRequest request,
                AdminForgotPasswordHandler handler,
                CancellationToken ct) =>
            {
                var validator = new AdminForgotPasswordValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<AdminForgotPasswordResponse>.Ok(response));
            })
            .WithName("AdminForgotPassword")
            .WithTags("Admin")
            .AllowAnonymous();
    }
}