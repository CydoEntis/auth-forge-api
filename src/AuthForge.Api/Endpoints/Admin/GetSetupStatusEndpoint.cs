using AuthForge.Api.Common.Responses;
using AuthForge.Application.Admin.Queries.GetSetupStatus;
using Mediator;

namespace AuthForge.Api.Endpoints.Admin;

public static class GetSetupStatusEndpoint
{
    public static void MapGetSetupStatusEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/setup/status", async (
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var query = new GetSetupStatusQuery();
                var result = await mediator.Send(query, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(ApiResponse<GetSetupStatusResponse>.SuccessResponse(result.Value!))
                    : Results.BadRequest(ApiResponse<GetSetupStatusResponse>.FailureResponse(result.Error!));
            })
            .AllowAnonymous()
            .WithName("GetSetupStatus")
            .WithTags("Admin Setup")
            .WithDescription("Check if initial admin setup is required")
            .Produces<ApiResponse<GetSetupStatusResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GetSetupStatusResponse>>(StatusCodes.Status400BadRequest);
    }
}