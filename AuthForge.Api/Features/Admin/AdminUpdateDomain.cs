using AuthForge.Api.Common;
using AuthForge.Api.Data;
using AuthForge.Api.Common.Exceptions.Http;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Admin;

public sealed record AdminUpdateDomainRequest(string AuthForgeDomain);

public sealed record AdminUpdateDomainResponse(string Message);

public class AdminUpdateDomainValidator : AbstractValidator<AdminUpdateDomainRequest>
{
    public AdminUpdateDomainValidator()
    {
        RuleFor(x => x.AuthForgeDomain)
            .NotEmpty()
            .WithMessage("Domain is required")
            .Must(BeValidUrl)
            .WithMessage("Must be a valid URL (e.g., https://auth.mycompany.com)");
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

public class AdminUpdateDomainHandler
{
    private readonly ConfigDbContext _configDb; // ✅ Changed from AppDbContext
    private readonly ILogger<AdminUpdateDomainHandler> _logger;

    public AdminUpdateDomainHandler(
        ConfigDbContext configDb, // ✅ Changed
        ILogger<AdminUpdateDomainHandler> logger)
    {
        _configDb = configDb;
        _logger = logger;
    }

    public async Task<AdminUpdateDomainResponse> HandleAsync(
        AdminUpdateDomainRequest request,
        CancellationToken ct)
    {
        var config = await _configDb.Configuration.FirstOrDefaultAsync(ct);

        if (config == null)
            throw new NotFoundException("Configuration not found");

        config.AuthForgeDomain = request.AuthForgeDomain.TrimEnd('/');
        config.UpdatedAtUtc = DateTime.UtcNow;

        await _configDb.SaveChangesAsync(ct);

        _logger.LogInformation("AuthForge domain updated to: {Domain}", config.AuthForgeDomain);

        return new AdminUpdateDomainResponse("Domain updated successfully");
    }
}

public static class AdminUpdateDomain
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPut($"{prefix}/admin/domain", async (
                AdminUpdateDomainRequest request,
                AdminUpdateDomainHandler handler,
                CancellationToken ct) =>
            {
                var validator = new AdminUpdateDomainValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<AdminUpdateDomainResponse>.Ok(response));
            })
            .WithName("AdminUpdateDomain")
            .WithTags("Admin")
            .RequireAuthorization();
    }
}