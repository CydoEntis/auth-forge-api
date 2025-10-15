// using AuthForge.Api.Common.Responses;
// using AuthForge.Api.Dtos.Auth;
// using AuthForge.Application.Auth.Commands.Refresh;
// using Mediator;
// using Microsoft.AspNetCore.Mvc;
//
// namespace AuthForge.Api.Endpoints.Auth;
//
// public static class RefreshEndpoint
// {
//     public static IEndpointRouteBuilder MapRefreshEndpoint(this IEndpointRouteBuilder app)
//     {
//         app.MapPost("/api/auth/refresh", HandleRefresh)
//             .WithName("RefreshToken")
//             .WithTags("Authentication")
//             .WithDescription("Refresh access token using refresh token")
//             .Produces<ApiResponse<RefreshTokenResponse>>(StatusCodes.Status200OK)
//             .Produces<ApiResponse<RefreshTokenResponse>>(StatusCodes.Status401Unauthorized)
//             .WithOpenApi();
//
//         return app;
//     }
//
//     private static async Task<IResult> HandleRefresh(
//         [FromBody] RefreshRequest request,
//         [FromServices] IMediator mediator,
//         HttpContext httpContext,
//         CancellationToken cancellationToken)
//     {
//         var command = new RefreshTokenCommand()
//         {
//             RefreshToken = request.RefreshToken,
//             IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
//             UserAgent = httpContext.Request.Headers.UserAgent.ToString()
//         };
//
//         var result = await mediator.Send(command, cancellationToken);
//
//         if (result.IsFailure)
//         {
//             var errorResponse = ApiResponse<RefreshTokenResponse>.FailureResponse(
//                 result.Error.Code,
//                 result.Error.Message);
//
//             return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
//         }
//
//         var successResponse = ApiResponse<RefreshTokenResponse>.SuccessResponse(result.Value);
//         return Results.Ok(successResponse);
//     }
// }