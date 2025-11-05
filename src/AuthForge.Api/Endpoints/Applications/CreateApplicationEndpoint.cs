using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Commands.CreateApplication;
using AuthForge.Application.Applications.Models;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Applications;

public static class CreateApplicationEndpoint
{
    public static IEndpointRouteBuilder MapCreateApplicationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/applications", Handle)
            .RequireAuthorization("Admin")
            .WithName("CreateApplication")
            .WithTags("Applications")
            .WithDescription("Create a new application with full configuration")
            .Produces<ApiResponse<CreateApplicationResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<CreateApplicationResponse>>(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateApplicationRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateApplicationCommand(
            request.Name,
            request.Description,
            request.AllowedOrigins,
            request.EmailSettings,
            request.OAuthSettings);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<CreateApplicationResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<CreateApplicationResponse>.SuccessResponse(result.Value);
        return Results.Created($"/api/applications/{result.Value.ApplicationId}", successResponse);
    }
}

public record CreateApplicationRequest(
    string Name,
    string? Description,
    List<string> AllowedOrigins,
    EmailSettingsRequest EmailSettings,
    OAuthSettingsRequest? OAuthSettings);