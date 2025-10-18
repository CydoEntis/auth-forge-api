using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Admin.Commands.Refresh;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Admin;

public static class RefreshAdminTokenEndpoint
{
    public static IEndpointRouteBuilder MapRefreshAdminTokenEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/refresh", HandleAsync)
            .AllowAnonymous()
            .WithName("AdminRefreshToken")
            .WithTags("Admin")
            .WithDescription("Refresh admin access token")
            .Produces<ApiResponse<RefreshAdminTokenResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<RefreshAdminTokenResponse>>(StatusCodes.Status401Unauthorized)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] RefreshAdminTokenRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RefreshAdminTokenCommand(request.RefreshToken);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<RefreshAdminTokenResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);
            
            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<RefreshAdminTokenResponse>.SuccessResponse(result.Value);
        
        return Results.Ok(successResponse);
    }
}

public record RefreshAdminTokenRequest(string RefreshToken);