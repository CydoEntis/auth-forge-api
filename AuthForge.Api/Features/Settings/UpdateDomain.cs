using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Settings;

public sealed record UpdateDomainRequest(string Domain);

public sealed record UpdateDomainResponse(string Message);

public class UpdateDomainValidator : AbstractValidator<UpdateDomainRequest>
{
    public UpdateDomainValidator()
    {
        RuleFor(x => x.Domain)
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

public class UpdateDomainHandler
{
    private readonly ConfigDbContext _configDb;
    private readonly ILogger<UpdateDomainHandler> _logger;

    public UpdateDomainHandler(
        ConfigDbContext configDb,
        ILogger<UpdateDomainHandler> logger)
    {
        _configDb = configDb;
        _logger = logger;
    }

    public async Task<UpdateDomainResponse> HandleAsync(
        UpdateDomainRequest request,
        CancellationToken ct)
    {
        var config = await _configDb.Configuration.FirstOrDefaultAsync(ct);

        if (config == null)
            throw new NotFoundException("Configuration not found");

        config.AuthForgeDomain = request.Domain.TrimEnd('/');
        config.UpdatedAtUtc = DateTime.UtcNow;

        await _configDb.SaveChangesAsync(ct);

        _logger.LogInformation("AuthForge domain updated to: {Domain}", config.AuthForgeDomain);

        return new UpdateDomainResponse("Domain updated successfully");
    }
}

public static class UpdateDomain
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPut($"{prefix}/domain", async (
                UpdateDomainRequest request,
                UpdateDomainHandler handler,
                CancellationToken ct) =>
            {
                var validator = new UpdateDomainValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<UpdateDomainResponse>.Ok(response));
            })
            .WithName("AdminUpdateDomain")
            .WithTags("Admin")
            .RequireAuthorization();
    }
}