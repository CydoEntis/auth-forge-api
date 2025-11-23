using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record UpdateApplicationSecurityRequest(
    int MaxFailedLoginAttempts,
    int LockoutDurationMinutes,
    int AccessTokenExpirationMinutes,
    int RefreshTokenExpirationDays
);

public sealed record UpdateApplicationSecurityResponse(
    Guid Id,
    string Name,
    int MaxFailedLoginAttempts,
    int LockoutDurationMinutes,
    int AccessTokenExpirationMinutes,
    int RefreshTokenExpirationDays,
    DateTime UpdatedAtUtc
);

public class UpdateApplicationSecurityValidator : AbstractValidator<UpdateApplicationSecurityRequest>
{
    public UpdateApplicationSecurityValidator()
    {
        RuleFor(x => x.MaxFailedLoginAttempts)
            .GreaterThan(0).WithMessage("Must be greater than 0")
            .LessThanOrEqualTo(20).WithMessage("Maximum 20 failed attempts");

        RuleFor(x => x.LockoutDurationMinutes)
            .GreaterThan(0).WithMessage("Must be greater than 0")
            .LessThanOrEqualTo(1440).WithMessage("Maximum 24 hours (1440 minutes)");

        RuleFor(x => x.AccessTokenExpirationMinutes)
            .GreaterThan(0).WithMessage("Must be greater than 0")
            .LessThanOrEqualTo(60).WithMessage("Maximum 60 minutes");

        RuleFor(x => x.RefreshTokenExpirationDays)
            .GreaterThan(0).WithMessage("Must be greater than 0")
            .LessThanOrEqualTo(90).WithMessage("Maximum 90 days");
    }
}

public class UpdateApplicationSecurityHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<UpdateApplicationSecurityHandler> _logger;

    public UpdateApplicationSecurityHandler(AppDbContext context, ILogger<UpdateApplicationSecurityHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UpdateApplicationSecurityResponse> HandleAsync(
        Guid id,
        UpdateApplicationSecurityRequest request,
        CancellationToken ct)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (application == null)
        {
            throw new NotFoundException($"Application with ID {id} not found");
        }

        application.MaxFailedLoginAttempts = request.MaxFailedLoginAttempts;
        application.LockoutDurationMinutes = request.LockoutDurationMinutes;
        application.AccessTokenExpirationMinutes = request.AccessTokenExpirationMinutes;
        application.RefreshTokenExpirationDays = request.RefreshTokenExpirationDays;
        application.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Updated security settings for application: {Name} ({Id})", application.Name,
            application.Id);

        return new UpdateApplicationSecurityResponse(
            application.Id,
            application.Name,
            application.MaxFailedLoginAttempts,
            application.LockoutDurationMinutes,
            application.AccessTokenExpirationMinutes,
            application.RefreshTokenExpirationDays,
            application.UpdatedAtUtc.Value
        );
    }
}

public static class UpdateApplicationSecurity
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPut($"{prefix}/applications/{{id:guid}}/security", async (
                Guid id,
                UpdateApplicationSecurityRequest request,
                [FromServices] UpdateApplicationSecurityHandler handler,
                CancellationToken ct) =>
            {
                var validator = new UpdateApplicationSecurityValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(id, request, ct);
                return Results.Ok(ApiResponse<UpdateApplicationSecurityResponse>.Ok(response));
            })
            .WithName("UpdateApplicationSecurity")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}