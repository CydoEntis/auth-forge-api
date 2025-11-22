using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Applications.Shared.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record UpdateApplicationRequest(
    string Name,
    string? Description,
    List<string>? AllowedOrigins,
    string? PasswordResetCallbackUrl,
    string? EmailVerificationCallbackUrl,
    string? MagicLinkCallbackUrl,
    bool IsActive
);

public sealed record UpdateApplicationResponse(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    DateTime UpdatedAtUtc
);

public class UpdateApplicationRequestValidator : AbstractValidator<UpdateApplicationRequest>
{
    public UpdateApplicationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Application name is required")
            .MinimumLength(3).WithMessage("Application name must be at least 3 characters")
            .MaximumLength(100).WithMessage("Application name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        When(x => x.AllowedOrigins != null && x.AllowedOrigins.Any(), () =>
        {
            RuleForEach(x => x.AllowedOrigins)
                .MustBeValidUrl();
            RuleFor(x => x.AllowedOrigins)
                .Must(x => x!.Count <= 10)
                .WithMessage("Maximum 10 allowed origins");
        });

        When(x => !string.IsNullOrEmpty(x.PasswordResetCallbackUrl), () =>
        {
            RuleFor(x => x.PasswordResetCallbackUrl)
                .MustBeValidUrl()
                .WithMessage("Password reset callback URL must be a valid URL");
        });

        When(x => !string.IsNullOrEmpty(x.EmailVerificationCallbackUrl), () =>
        {
            RuleFor(x => x.EmailVerificationCallbackUrl)
                .MustBeValidUrl()
                .WithMessage("Email verification callback URL must be a valid URL");
        });

        When(x => !string.IsNullOrEmpty(x.MagicLinkCallbackUrl), () =>
        {
            RuleFor(x => x.MagicLinkCallbackUrl)
                .MustBeValidUrl()
                .WithMessage("Magic link callback URL must be a valid URL");
        });
    }
}

public class UpdateApplicationHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<UpdateApplicationHandler> _logger;

    public UpdateApplicationHandler(AppDbContext context, ILogger<UpdateApplicationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UpdateApplicationResponse> HandleAsync(
        Guid id,
        UpdateApplicationRequest request,
        CancellationToken ct)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (application == null)
        {
            _logger.LogWarning("Application not found for update: {Id}", id);
            throw new NotFoundException($"Application with ID {id} not found");
        }

        application.Name = request.Name;
        application.Description = request.Description;
        application.IsActive = request.IsActive;

        if (request.AllowedOrigins != null)
        {
            application.AllowedOrigins = request.AllowedOrigins;
        }

        application.PasswordResetCallbackUrl = request.PasswordResetCallbackUrl;
        application.EmailVerificationCallbackUrl = request.EmailVerificationCallbackUrl;
        application.MagicLinkCallbackUrl = request.MagicLinkCallbackUrl;

        application.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Application updated: {Name} ({Id})",
            application.Name, application.Id);

        return new UpdateApplicationResponse(
            application.Id,
            application.Name,
            application.Slug,
            application.IsActive,
            application.UpdatedAtUtc.Value
        );
    }
}

public static class UpdateApplication
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPut($"{prefix}/applications/{{id:guid}}", async (
                Guid id,
                UpdateApplicationRequest request,
                UpdateApplicationHandler handler,
                CancellationToken ct) =>
            {
                var validator = new UpdateApplicationRequestValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(id, request, ct);
                return Results.Ok(ApiResponse<UpdateApplicationResponse>.Ok(response));
            })
            .WithName("UpdateApplication")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}