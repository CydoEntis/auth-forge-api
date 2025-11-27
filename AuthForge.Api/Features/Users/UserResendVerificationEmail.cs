using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Users;

public sealed record UserResendVerificationEmailRequest(string Email);

public sealed record UserResendVerificationEmailResponse(string Message);

public sealed class UserResendVerificationEmailValidator : AbstractValidator<UserResendVerificationEmailRequest>
{
    public UserResendVerificationEmailValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}

public sealed class UserResendVerificationEmailHandler
{
    private readonly AppDbContext _context;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IEmailServiceFactory _emailServiceFactory;
    private readonly IJwtService _jwtService;
    private readonly ILogger<UserResendVerificationEmailHandler> _logger;

    public UserResendVerificationEmailHandler(
        AppDbContext context,
        IEmailTemplateService emailTemplateService,
        IEmailServiceFactory emailServiceFactory,
        IJwtService jwtService,
        ILogger<UserResendVerificationEmailHandler> logger)
    {
        _context = context;
        _emailTemplateService = emailTemplateService;
        _emailServiceFactory = emailServiceFactory;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<UserResendVerificationEmailResponse> HandleAsync(
        Guid applicationId,
        UserResendVerificationEmailRequest emailRequest,
        CancellationToken ct)
    {
        var app = await _context.Applications
            .Include(a => a.EmailSettings)
            .FirstOrDefaultAsync(a => a.Id == applicationId && !a.IsDeleted, ct);

        if (app == null || !app.IsActive)
        {
            _logger.LogWarning(
                "Resend verification attempt for invalid application: {AppId}",
                applicationId);
            throw new BadRequestException("Application not found or inactive");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.ApplicationId == applicationId
                && u.Email == emailRequest.Email, ct);

        if (user == null)
        {
            _logger.LogWarning(
                "Resend verification requested for non-existent email {Email} in app {AppId}",
                emailRequest.Email,
                applicationId);
            return new UserResendVerificationEmailResponse(
                "If an account exists with that email, a verification email has been sent.");
        }

        if (user.EmailVerified)
        {
            _logger.LogInformation(
                "Resend verification requested for already verified email {Email}",
                user.Email);
            return new UserResendVerificationEmailResponse(
                "If an account exists with that email, a verification email has been sent.");
        }

        var verificationToken = _jwtService.GenerateUrlSafeToken(32);
        user.EmailVerificationToken = verificationToken;
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        try
        {
            await SendVerificationEmailAsync(user, app, verificationToken, ct);
            _logger.LogInformation(
                "Verification email resent to {Email}",
                user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
            throw new InvalidOperationException("Failed to send verification email");
        }

        return new UserResendVerificationEmailResponse(
            "If an account exists with that email, a verification email has been sent.");
    }

    private async Task SendVerificationEmailAsync(
        Entities.User user,
        Entities.Application app,
        string verificationToken,
        CancellationToken ct)
    {
        var verificationUrl = $"{app.EmailVerificationCallbackUrl}?token={verificationToken}";

        var emailMessage = await _emailTemplateService.CreateEmailVerificationEmailAsync(
            toEmail: user.Email,
            toName: user.FirstName ?? "User",
            verificationUrl: verificationUrl,
            appName: app.Name
        );

        var (fromAddress, fromName) = await _emailServiceFactory.GetFromDetailsForApplicationAsync(app, ct);

        var finalMessage = emailMessage with
        {
            From = fromAddress,
            FromName = fromName ?? app.Name
        };

        var emailService = await _emailServiceFactory.CreateForApplicationAsync(app, ct);
        var result = await emailService.SendAsync(finalMessage, ct);

        if (!result.Success)
        {
            _logger.LogError("Failed to send verification email: {Error}", result.ErrorMessage);
            throw new InvalidOperationException("Failed to send verification email");
        }
    }
}

public static class UserResendVerificationEmail
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/apps/{{appId:guid}}/auth/resend-verification", async (
                Guid appId,
                UserResendVerificationEmailRequest emailRequest,
                UserResendVerificationEmailHandler emailHandler,
                CancellationToken ct) =>
            {
                var validator = new UserResendVerificationEmailValidator();
                var validationResult = await validator.ValidateAsync(emailRequest, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await emailHandler.HandleAsync(appId, emailRequest, ct);
                return Results.Ok(ApiResponse<UserResendVerificationEmailResponse>.Ok(response));
            })
            .WithName("UserResendVerification")
            .WithTags("User Auth")
            .AllowAnonymous();
    }
}