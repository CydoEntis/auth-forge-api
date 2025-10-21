using AuthForge.Api.Attributes;
using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.VerifyEmail;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class VerifyEmailEndpoint
{
    public static IEndpointRouteBuilder MapVerifyEmailEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/endusers/verify-email", Handle)
            .WithMetadata(new RateLimitAttribute(10, 60))
            .WithName("VerifyEmail")
            .WithTags("EndUsers")
            .WithDescription("Verify email using verification token")
            .Produces<ApiResponse<VerifyEmailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<VerifyEmailResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<VerifyEmailResponse>>(StatusCodes.Status401Unauthorized)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        [FromBody] VerifyEmailRequest request,
        HttpContext httpContext,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var application = httpContext.Items["Application"] as Domain.Entities.Application;

        if (application is null)
        {
            var errorResponse = ApiResponse<VerifyEmailResponse>.FailureResponse(
                EndUserErrors.InvalidApiKey.Code,
                EndUserErrors.InvalidApiKey.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
        }

        var command = new VerifyEmailCommand(
            application.Id,
            Email.Create(request.Email),
            request.VerificationToken);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<VerifyEmailResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<VerifyEmailResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record VerifyEmailRequest(
    string Email,
    string VerificationToken);