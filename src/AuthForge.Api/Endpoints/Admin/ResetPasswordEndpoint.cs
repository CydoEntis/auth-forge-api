using AuthForge.Api.Attributes;
using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Admin.Commands.ResetPassword;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Admin;

public static class ResetAdminPasswordEndpoint
{
    public static IEndpointRouteBuilder MapResetAdminPasswordEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/reset-password", Handle)
            .WithMetadata(new RateLimitAttribute(5, 300)) 
            .AllowAnonymous()
            .WithName("ResetAdminPassword")
            .WithTags("Admin")
            .WithDescription("Reset admin password with token")
            .Produces<ApiResponse<ResetAdminPasswordResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ResetAdminPasswordResponse>>(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        [FromBody] ResetAdminPasswordRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ResetAdminPasswordCommand(
            request.ResetToken,
            request.NewPassword,
            request.ConfirmPassword);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<ResetAdminPasswordResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<ResetAdminPasswordResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record ResetAdminPasswordRequest(
    string ResetToken,
    string NewPassword,
    string ConfirmPassword);