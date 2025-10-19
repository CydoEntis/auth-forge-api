// src/AuthForge.Api/Endpoints/EndUsers/ChangePasswordEndpoint.cs

using System.IdentityModel.Tokens.Jwt;
using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.ChangePassword;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class ChangePasswordEndpoint
{
    public static IEndpointRouteBuilder MapChangePasswordEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/endusers/change-password", Handle)
            .RequireAuthorization("EndUser")
            .WithName("ChangePassword")
            .WithTags("EndUsers")
            .WithDescription("Change password for authenticated end user")
            .Produces<ApiResponse<ChangePasswordResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ChangePasswordResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<ChangePasswordResponse>>(StatusCodes.Status401Unauthorized)
            .WithOpenApi();

        return app;
    }


    private static async Task<IResult> Handle(
        [FromBody] ChangePasswordRequest request,
        HttpContext httpContext,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            var errorResponse = ApiResponse<ChangePasswordResponse>.FailureResponse(
                EndUserErrors.Unauthorized.Code,
                EndUserErrors.Unauthorized.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
        }

        var command = new ChangePasswordCommand(
            EndUserId.Create(userId),
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmPassword);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<ChangePasswordResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<ChangePasswordResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);