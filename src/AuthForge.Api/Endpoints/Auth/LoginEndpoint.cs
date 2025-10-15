// using AuthForge.Api.Common.Responses;
// using AuthForge.Api.Dtos.Auth;
// using AuthForge.Application.Auth.Commands.Login;
// using Mediator;
// using Microsoft.AspNetCore.Mvc;
//
// namespace AuthForge.Api.Endpoints.Auth;
//
// public static class LoginEndpoint
// {
//     public static IEndpointRouteBuilder MapLoginEndpoint(this IEndpointRouteBuilder app)
//     {
//         app.MapPost("/api/auth/login", HandleLogin)
//             .WithName("Login")
//             .WithTags("Authentication")
//             .WithDescription("Authenticate user and receive JWT tokens")
//             .Produces<ApiResponse<LoginResponse>>(StatusCodes.Status200OK)
//             .Produces<ApiResponse<LoginResponse>>(StatusCodes.Status401Unauthorized)
//             .WithOpenApi();
//
//         return app;
//     }
//
//     private static async Task<IResult> HandleLogin(
//         [FromBody] LoginRequest request,
//         [FromServices] IMediator mediator,
//         HttpContext httpContext,
//         CancellationToken cancellationToken)
//     {
//         var command = new LoginCommand
//         {
//             TenantId = request.TenantId,
//             Email = request.Email,
//             Password = request.Password,
//             IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
//             UserAgent = httpContext.Request.Headers.UserAgent.ToString()
//         };
//
//         var result = await mediator.Send(command, cancellationToken);
//
//         if (result.IsFailure)
//         {
//             var errorResponse = ApiResponse<LoginResponse>.FailureResponse(
//                 result.Error.Code,
//                 result.Error.Message);
//
//             return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
//         }
//
//         var successResponse = ApiResponse<LoginResponse>.SuccessResponse(result.Value);
//         return Results.Ok(successResponse);
//     }
// }
