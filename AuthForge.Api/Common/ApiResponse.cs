namespace AuthForge.Api.Common;

// Standard API response for operations that return data.
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data) =>
        new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(string code, string message, List<FieldError>? fieldErrors = null) =>
        new()
        {
            Success = false,
            Error = new ApiError
            {
                Code = code,
                Message = message,
                FieldErrors = fieldErrors
            }
        };
}

public class ApiResponse
{
    public bool Success { get; init; }
    public ApiError? Error { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse Ok() =>
        new() { Success = true };

    public static ApiResponse Fail(string code, string message, List<FieldError>? fieldErrors = null) =>
        new()
        {
            Success = false,
            Error = new ApiError
            {
                Code = code,
                Message = message,
                FieldErrors = fieldErrors
            }
        };
}

public class ApiError
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public List<FieldError>? FieldErrors { get; init; }
}

public class FieldError
{
    public string Field { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}