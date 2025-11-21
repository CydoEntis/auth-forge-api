using System.Net;
using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions;
using FluentValidation;

namespace AuthForge.Api.Middleware;

public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException vex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var fieldErrors = vex.Errors.Select(e => new FieldError
            {
                Field = char.ToLowerInvariant(e.PropertyName[0]) + e.PropertyName.Substring(1),
                Code = e.ErrorCode ?? ErrorCodes.ValidationFailed,
                Message = e.ErrorMessage
            }).ToList();

            var response = ApiResponse<object>.Fail(ErrorCodes.ValidationFailed, "Validation failed", fieldErrors);

            _logger.LogWarning("Validation failed: {Errors}",
                string.Join(", ", fieldErrors.Select(e => $"{e.Field}: {e.Message}")));

            await context.Response.WriteAsJsonAsync(response);
        }
        catch (ApiException aex)
        {
            context.Response.StatusCode = (int)aex.StatusCode;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Fail(aex.Code, aex.Message);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Fail(ErrorCodes.InternalError, "An unexpected error occurred");
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}