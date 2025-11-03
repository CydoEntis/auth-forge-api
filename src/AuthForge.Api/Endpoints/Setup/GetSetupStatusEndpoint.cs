using AuthForge.Api.Common.Responses;
using AuthForge.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthForge.Api.Endpoints.Setup;

public static class GetSetupStatusEndpoint
{
    public static void MapGetSetupStatusEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/setup/status", async (
                ISetupService setupService,
                ILogger<ISetupService> logger,
                CancellationToken cancellationToken) =>
            {
                logger.LogInformation("Setup status check requested");

                var isComplete = await setupService.IsSetupCompleteAsync();

                logger.LogInformation("Setup complete status: {IsComplete}", isComplete);

                var response = new SetupStatusResponse(isComplete);

                logger.LogInformation("Returning response: {@Response}", response);

                return Results.Ok(ApiResponse<SetupStatusResponse>.SuccessResponse(response));
            })
            .AllowAnonymous()
            .WithName("GetSetupStatus")
            .WithTags("Setup")
            .WithDescription("Check if initial setup is required")
            .Produces<ApiResponse<SetupStatusResponse>>(StatusCodes.Status200OK)
            .WithOpenApi();
    }
}

public record SetupStatusResponse(bool IsComplete);