using AuthForge.Api.Common.Responses;
using AuthForge.Application.Setup.Commands.TestDatabaseConnection;
using AuthForge.Domain.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Setup;

public static class TestDatabaseConnectionEndpoint
{
    public static void MapTestDatabaseConnectionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/setup/test-database", Handle)
            .AllowAnonymous()
            .WithName("TestDatabaseConnection")
            .WithTags("Setup")
            .WithDescription("Test database connection before completing setup")
            .Produces<ApiResponse<TestDatabaseConnectionResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<TestDatabaseConnectionResponse>>(StatusCodes.Status400BadRequest)
            .WithOpenApi();
    }

    private static async Task<IResult> Handle(
        [FromBody] TestDatabaseConnectionRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new TestDatabaseConnectionCommand(
            request.DatabaseType,
            request.ConnectionString);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<TestDatabaseConnectionResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            return Results.BadRequest(errorResponse);
        }

        var successResponse = ApiResponse<TestDatabaseConnectionResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record TestDatabaseConnectionRequest(
    DatabaseType DatabaseType,
    string? ConnectionString);