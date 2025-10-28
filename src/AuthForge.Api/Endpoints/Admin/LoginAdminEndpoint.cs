using AuthForge.Api.Attributes;
using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Admin.Commands.Login;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Admin;

public static class LoginAdminEndpoint
{
    public static IEndpointRouteBuilder MapLoginAdminEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/login", HandleAsync)
            // .WithMetadata(new RateLimitAttribute(5, 1)) 
            .AllowAnonymous()
            .WithName("AdminLogin")
            .WithTags("Admin")
            .WithDescription("Login as admin user")
            .Produces<ApiResponse<LoginAdminResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LoginAdminResponse>>(StatusCodes.Status401Unauthorized)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] LoginAdminRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LoginAdminCommand(request.Email, request.Password);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<LoginAdminResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);
            
            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<LoginAdminResponse>.SuccessResponse(result.Value);
        
        return Results.Ok(successResponse);
    }
}

public record LoginAdminRequest(string Email, string Password);