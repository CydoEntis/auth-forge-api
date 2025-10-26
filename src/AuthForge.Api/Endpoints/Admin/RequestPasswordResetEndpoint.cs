using AuthForge.Api.Attributes;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Admin.Commands.RequestPasswordReset;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Admin;

public static class RequestAdminPasswordResetEndpoint
{
    public static IEndpointRouteBuilder MapRequestAdminPasswordResetEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/forgot-password", Handle)
            .WithMetadata(new RateLimitAttribute(3, 300))
            .AllowAnonymous()
            .WithName("RequestAdminPasswordReset")
            .WithTags("Admin")
            .WithDescription("Request admin password reset email")
            .Produces<ApiResponse<RequestAdminPasswordResetResponse>>(StatusCodes.Status200OK)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        [FromBody] RequestAdminPasswordResetRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RequestAdminPasswordResetCommand(request.Email);
        var result = await mediator.Send(command, cancellationToken);

        var response = ApiResponse<RequestAdminPasswordResetResponse>.SuccessResponse(result.Value);
        return Results.Ok(response);
    }
}

public record RequestAdminPasswordResetRequest(string Email);