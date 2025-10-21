using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.SendVerificationEmail;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthForge.Api.Attributes;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class SendVerificationEmailEndpoint
{
    public static IEndpointRouteBuilder MapSendVerificationEmailEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/endusers/send-verification-email", Handle)
            .WithMetadata(new RateLimitAttribute(3, 60))
            .RequireAuthorization("EndUser")
            .WithName("SendVerificationEmail")
            .WithTags("EndUsers")
            .WithDescription("Send email verification token to authenticated user")
            .Produces<ApiResponse<SendVerificationEmailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SendVerificationEmailResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<SendVerificationEmailResponse>>(StatusCodes.Status401Unauthorized)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        HttpContext httpContext,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            var errorResponse = ApiResponse<SendVerificationEmailResponse>.FailureResponse(
                EndUserErrors.Unauthorized.Code,
                EndUserErrors.Unauthorized.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
        }

        var command = new SendVerificationEmailCommand(EndUserId.Create(userId));

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<SendVerificationEmailResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<SendVerificationEmailResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}