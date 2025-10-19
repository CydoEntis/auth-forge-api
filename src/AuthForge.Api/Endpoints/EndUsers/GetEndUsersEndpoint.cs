using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Common.Models;
using AuthForge.Application.EndUsers.Models;
using AuthForge.Application.EndUsers.Queries.GetAll;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class GetEndUsersEndpoint
{
    public static IEndpointRouteBuilder MapGetEndUsersEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/applications/{applicationId}/users", HandleAsync)
            .RequireAuthorization("Admin")
            .WithName("GetEndUsers")
            .WithTags("EndUsers")
            .WithDescription("Get paginated list of end users for an application")
            .Produces<ApiResponse<PagedResponse<EndUserSummary>>>(StatusCodes.Status200OK)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] string applicationId,
        [AsParameters] EndUserFilterParameters parameters,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEndUsersQuery(applicationId, parameters);  
        
        var result = await mediator.Send(query, cancellationToken);
        
        if (result.IsFailure) 
        {
            var errorResponse = ApiResponse<PagedResponse<EndUserSummary>>.FailureResponse(
                result.Error.Code,
                result.Error.Message);
            
            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<PagedResponse<EndUserSummary>>.SuccessResponse(result.Value); 
        return Results.Ok(successResponse);
    }
}