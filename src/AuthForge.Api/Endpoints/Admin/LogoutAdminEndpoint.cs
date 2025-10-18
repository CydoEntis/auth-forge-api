using AuthForge.Api.Common.Responses;
using AuthForge.Application.Admin.Commands.Logout;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Admin;

public static class LogoutAdminEndpoint
{
    public static IEndpointRouteBuilder MapLogoutAdminEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/logout", HandleAsync)
            .RequireAuthorization("Admin")
            .WithName("AdminLogout")
            .WithTags("Admin")
            .WithDescription("Logout admin and revoke all refresh tokens")
            .Produces<ApiResponse<LogoutAdminResponse>>(StatusCodes.Status200OK)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LogoutAdminCommand();
        var result = await mediator.Send(command, cancellationToken);

        var successResponse = ApiResponse<LogoutAdminResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}