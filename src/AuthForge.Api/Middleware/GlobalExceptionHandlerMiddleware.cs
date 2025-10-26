using AuthForge.Api.Common.Responses;
using FluentValidation;
using System.Text.Json;

namespace AuthForge.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception occurred while processing {Method} {Path} from {IP}. User: {UserId}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress,
                context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous");

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        ApiResponse<object> response;
        int statusCode;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = StatusCodes.Status400BadRequest;
                response = ApiResponse<object>.FailureResponse(
                    code: "Validation.Failed",
                    message: string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage))
                );
                _logger.LogWarning(
                    "Validation exception: {ValidationErrors}",
                    string.Join(", ", validationException.Errors.Select(e => e.ErrorMessage)));
                break;

            case ArgumentException argumentException:
                statusCode = StatusCodes.Status400BadRequest;
                response = ApiResponse<object>.FailureResponse(
                    code: "Argument.Invalid",
                    message: _environment.IsDevelopment()
                        ? argumentException.Message
                        : "Invalid input provided."
                );
                _logger.LogWarning(argumentException, "Argument exception occurred");
                break;

            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status401Unauthorized;
                response = ApiResponse<object>.FailureResponse(
                    code: "Auth.Unauthorized",
                    message: "You are not authorized to perform this action."
                );
                _logger.LogWarning("Unauthorized access attempt");
                break;

            case KeyNotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                response = ApiResponse<object>.FailureResponse(
                    code: "Resource.NotFound",
                    message: "The requested resource was not found."
                );
                _logger.LogWarning("Resource not found");
                break;

            case InvalidOperationException invalidOpException when invalidOpException.Message.Contains("database"):
                statusCode = StatusCodes.Status503ServiceUnavailable;
                response = ApiResponse<object>.FailureResponse(
                    code: "Database.Unavailable",
                    message: "Database is temporarily unavailable. Please try again later."
                );
                _logger.LogError(invalidOpException, "Database operation failed");
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                response = ApiResponse<object>.FailureResponse(
                    code: "Internal.ServerError",
                    message: _environment.IsDevelopment()
                        ? $"An unexpected error occurred: {exception.Message}" 
                        : "An unexpected error occurred. Please try again later." 
                );
                break;
        }

        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}