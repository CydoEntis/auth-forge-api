// src/AuthForge.Api/Endpoints/EndUsers/ForgotPasswordEndpoint.cs

using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.ForgotPassword;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class ForgotPasswordEndpoint
{
    public static IEndpointRouteBuilder MapForgotPasswordEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/endusers/forgot-password", Handle)
            .WithName("ForgotPassword")
            .WithTags("EndUsers")
            .WithDescription("Request a password reset token")
            .Produces<ApiResponse<ForgotPasswordResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ForgotPasswordResponse>>(StatusCodes.Status401Unauthorized)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        [FromBody] ForgotPasswordRequest request,
        HttpContext httpContext,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var application = httpContext.Items["Application"] as Domain.Entities.Application;

        if (application is null)
        {
            var errorResponse = ApiResponse<ForgotPasswordResponse>.FailureResponse(
                EndUserErrors.InvalidApiKey.Code,
                EndUserErrors.InvalidApiKey.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
        }

        var command = new ForgotPasswordCommand(
            application.Id,
            Email.Create(request.Email));

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<ForgotPasswordResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<ForgotPasswordResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record ForgotPasswordRequest(string Email);