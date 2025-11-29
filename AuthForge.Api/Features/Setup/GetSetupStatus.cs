using AuthForge.Api.Common;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Setup;

public record GetSetupStatusResponse(
    bool IsSetupComplete,
    string Message);

public class GetSetupStatusHandler
{
    private readonly ConfigDbContext _configDb;

    public GetSetupStatusHandler(ConfigDbContext configDb)
    {
        _configDb = configDb;
    }

    public async Task<GetSetupStatusResponse> HandleAsync(CancellationToken ct)
    {
        var config = await _configDb.Configuration.FirstOrDefaultAsync(ct);

        var isSetupComplete = config?.IsSetupComplete ?? false;

        return new GetSetupStatusResponse(
            IsSetupComplete: isSetupComplete,
            Message: isSetupComplete
                ? "Setup is complete. Application is ready to use."
                : "Setup is required. Please complete the setup wizard to continue.");
    }
}

public static class GetSetupStatusFeature
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}/status", async (GetSetupStatusHandler handler, CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(ct);
                return Results.Ok(ApiResponse<GetSetupStatusResponse>.Ok(response));
            })
            .WithName("GetSetupStatus")
            .WithTags("Setup")
            .AllowAnonymous();
    }
}