using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.ResetPassword;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class ResetPasswordEndpoint
{
    public static IEndpointRouteBuilder MapResetPasswordEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/endusers/reset-password", Handle)
            .WithName("ResetPassword")
            .WithTags("EndUsers")
            .WithDescription("Reset password using reset token")
            .Produces<ApiResponse<ResetPasswordResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ResetPasswordResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<ResetPasswordResponse>>(StatusCodes.Status401Unauthorized)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        [FromBody] ResetPasswordRequest request,
        HttpContext httpContext,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var application = httpContext.Items["Application"] as Domain.Entities.Application;

        if (application is null)
        {
            var errorResponse = ApiResponse<ResetPasswordResponse>.FailureResponse(
                EndUserErrors.InvalidApiKey.Code,
                EndUserErrors.InvalidApiKey.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
        }

        var command = new ResetPasswordCommand(
            application.Id,
            Email.Create(request.Email),
            request.ResetToken,
            request.NewPassword,
            request.ConfirmPassword);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<ResetPasswordResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<ResetPasswordResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record ResetPasswordRequest(
    string Email,
    string ResetToken,
    string NewPassword,
    string ConfirmPassword);