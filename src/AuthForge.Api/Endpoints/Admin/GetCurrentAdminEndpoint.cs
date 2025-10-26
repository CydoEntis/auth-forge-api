using AuthForge.Api.Common.Responses;
using AuthForge.Application.Admin.Queries.GetCurrentAdmin;
using Mediator;

namespace AuthForge.Api.Endpoints.Admin;

public static class GetCurrentAdminEndpoint
{
    public static void MapGetCurrentAdminEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/me", async (
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var query = new GetCurrentAdminQuery();
                var result = await mediator.Send(query, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(ApiResponse<GetCurrentAdminResponse>.SuccessResponse(result.Value!))
                    : Results.BadRequest(ApiResponse<GetCurrentAdminResponse>.FailureResponse(result.Error!));
            })
            .RequireAuthorization("Admin")
            .WithName("GetCurrentAdmin")
            .WithTags("Admin")
            .Produces<ApiResponse<GetCurrentAdminResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GetCurrentAdminResponse>>(StatusCodes.Status401Unauthorized);
    }
}