using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record UpdateApplicationOAuthRequest(
    bool GoogleEnabled,
    string? GoogleClientId,
    string? GoogleClientSecret,
    bool GithubEnabled,
    string? GithubClientId,
    string? GithubClientSecret
);

public sealed record UpdateApplicationOAuthResponse(
    Guid Id,
    string Name,
    bool GoogleEnabled,
    string? GoogleClientId,
    bool GithubEnabled,
    string? GithubClientId,
    DateTime UpdatedAtUtc
);

public class UpdateApplicationOAuthValidator : AbstractValidator<UpdateApplicationOAuthRequest>
{
    public UpdateApplicationOAuthValidator()
    {
        When(x => x.GoogleEnabled, () =>
        {
            RuleFor(x => x.GoogleClientId)
                .NotEmpty().WithMessage("Google Client ID is required when Google OAuth is enabled");
            RuleFor(x => x.GoogleClientSecret)
                .NotEmpty().WithMessage("Google Client Secret is required when Google OAuth is enabled");
        });

        When(x => x.GithubEnabled, () =>
        {
            RuleFor(x => x.GithubClientId)
                .NotEmpty().WithMessage("Github Client ID is required when Github OAuth is enabled");
            RuleFor(x => x.GithubClientSecret)
                .NotEmpty().WithMessage("Github Client Secret is required when Github OAuth is enabled");
        });
    }
}

public class UpdateApplicationOAuthHandler
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<UpdateApplicationOAuthHandler> _logger;

    public UpdateApplicationOAuthHandler(
        AppDbContext context,
        IEncryptionService encryptionService,
        ILogger<UpdateApplicationOAuthHandler> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<UpdateApplicationOAuthResponse> HandleAsync(
        Guid id,
        UpdateApplicationOAuthRequest request,
        CancellationToken ct)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (application == null)
        {
            throw new NotFoundException($"Application with ID {id} not found");
        }

        application.GoogleEnabled = request.GoogleEnabled;
        if (request.GoogleEnabled)
        {
            application.GoogleClientId = request.GoogleClientId;
            if (!string.IsNullOrEmpty(request.GoogleClientSecret))
            {
                application.GoogleClientSecretEncrypted = _encryptionService.Encrypt(request.GoogleClientSecret);
            }
        }
        else
        {
            application.GoogleClientId = null;
            application.GoogleClientSecretEncrypted = null;
        }

        application.GithubEnabled = request.GithubEnabled;
        if (request.GithubEnabled)
        {
            application.GithubClientId = request.GithubClientId;
            if (!string.IsNullOrEmpty(request.GithubClientSecret))
            {
                application.GithubClientSecretEncrypted = _encryptionService.Encrypt(request.GithubClientSecret);
            }
        }
        else
        {
            application.GithubClientId = null;
            application.GithubClientSecretEncrypted = null;
        }

        application.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Updated OAuth settings for application: {Name} ({Id})", application.Name,
            application.Id);

        return new UpdateApplicationOAuthResponse(
            application.Id,
            application.Name,
            application.GoogleEnabled,
            application.GoogleClientId,
            application.GithubEnabled,
            application.GithubClientId,
            application.UpdatedAtUtc.Value
        );
    }
}

public static class UpdateApplicationOAuth
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPut($"{prefix}/applications/{{id:guid}}/oauth", async (
                Guid id,
                UpdateApplicationOAuthRequest request,
                [FromServices] UpdateApplicationOAuthHandler handler,
                CancellationToken ct) =>
            {
                var validator = new UpdateApplicationOAuthValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(id, request, ct);
                return Results.Ok(ApiResponse<UpdateApplicationOAuthResponse>.Ok(response));
            })
            .WithName("UpdateApplicationOAuth")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}