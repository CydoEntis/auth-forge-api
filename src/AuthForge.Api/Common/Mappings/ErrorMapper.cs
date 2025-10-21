using AuthForge.Domain.Errors;

namespace AuthForge.Api.Common.Mappings;

public static class ErrorMapper
{
    public static int ToStatusCode(Error error)
    {
        // Application errors
        if (error.Code == ApplicationErrors.NotFound.Code)
            return StatusCodes.Status404NotFound;
        if (error.Code == ApplicationErrors.Unauthorized.Code)
            return StatusCodes.Status403Forbidden;
        if (error.Code == ApplicationErrors.Inactive.Code)
            return StatusCodes.Status403Forbidden;
        if (error.Code == ApplicationErrors.SlugAlreadyExists.Code)
            return StatusCodes.Status409Conflict;
        if (error.Code == ApplicationErrors.InvalidSettings.Code)
            return StatusCodes.Status400BadRequest;
        if (error.Code == ApplicationErrors.InvalidId.Code)
            return StatusCodes.Status400BadRequest;
        if (error.Code == ApplicationErrors.InvalidOrigin.Code)
            return StatusCodes.Status400BadRequest;
        if (error.Code == ApplicationErrors.OriginNotFound.Code)
            return StatusCodes.Status404NotFound;
        if (error.Code == ApplicationErrors.OriginAlreadyExists.Code)
            return StatusCodes.Status409Conflict;
        if (error.Code.StartsWith("Application.OriginError"))
            return StatusCodes.Status400BadRequest;

        // EndUser errors
        if (error.Code == EndUserErrors.NotFound.Code)
            return StatusCodes.Status404NotFound;
        if (error.Code == EndUserErrors.InvalidCredentials.Code)
            return StatusCodes.Status401Unauthorized;
        if (error.Code == EndUserErrors.DuplicateEmail.Code)
            return StatusCodes.Status409Conflict;
        if (error.Code == EndUserErrors.Inactive.Code)
            return StatusCodes.Status403Forbidden;
        if (error.Code == EndUserErrors.LockedOut.Code)
            return StatusCodes.Status403Forbidden;
        if (error.Code == EndUserErrors.EmailNotVerified.Code)
            return StatusCodes.Status403Forbidden;
        if (error.Code == EndUserErrors.InvalidEmailVerificationToken.Code)
            return StatusCodes.Status400BadRequest;
        if (error.Code == EndUserErrors.InvalidApiKey.Code)
            return StatusCodes.Status401Unauthorized;
        if (error.Code == EndUserErrors.InvalidResetToken.Code)
            return StatusCodes.Status400BadRequest;
        if (error.Code == EndUserErrors.Unauthorized.Code)
            return StatusCodes.Status403Forbidden;
        if (error.Code == EndUserErrors.EmailAlreadyVerified.Code)
            return StatusCodes.Status400BadRequest;
        if (error.Code == EndUserErrors.InvalidVerificationToken.Code)
            return StatusCodes.Status400BadRequest;
        if (error.Code == EndUserErrors.InvalidId.Code)
            return StatusCodes.Status400BadRequest;
        if (error.Code == EndUserErrors.AlreadyActive.Code)
            return StatusCodes.Status400BadRequest;
        if (error.Code == EndUserErrors.AlreadyDeactivated.Code)
            return StatusCodes.Status400BadRequest;
        if (error.Code == EndUserErrors.AlreadyLockedOut.Code)
            return StatusCodes.Status400BadRequest;
        if (error.Code == EndUserErrors.NotLockedOut.Code)
            return StatusCodes.Status400BadRequest;

        // EndUser Token errors
        if (error.Code == EndUserRefreshTokenErrors.Expired.Code)
            return StatusCodes.Status401Unauthorized;
        if (error.Code == EndUserRefreshTokenErrors.Revoked.Code)
            return StatusCodes.Status401Unauthorized;
        if (error.Code == EndUserRefreshTokenErrors.Invalid.Code)
            return StatusCodes.Status401Unauthorized;
        if (error.Code == EndUserRefreshTokenErrors.NotFound.Code)
            return StatusCodes.Status404NotFound;

        // Password Reset errors
        if (error.Code == PasswordResetErrors.TokenNotFound.Code)
            return StatusCodes.Status404NotFound;
        if (error.Code == PasswordResetErrors.TokenExpired.Code)
            return StatusCodes.Status401Unauthorized;
        if (error.Code == PasswordResetErrors.TokenAlreadyUsed.Code)
            return StatusCodes.Status400BadRequest;
        if (error.Code == PasswordResetErrors.InvalidToken.Code)
            return StatusCodes.Status401Unauthorized;

        // Authentication errors
        if (error.Code == AuthenticationErrors.InvalidToken.Code)
            return StatusCodes.Status401Unauthorized;
        if (error.Code == AuthenticationErrors.TokenExpired.Code)
            return StatusCodes.Status401Unauthorized;
        if (error.Code == AuthenticationErrors.Unauthorized.Code)
            return StatusCodes.Status403Forbidden;

        // Admin errors
        if (error.Code == AdminErrors.InvalidCredentials.Code)
            return StatusCodes.Status401Unauthorized;

        // Rate Limit errors
        if (error.Code.StartsWith("RateLimit."))
            return StatusCodes.Status429TooManyRequests;

        // Validation errors
        if (error.Code.StartsWith("Validation."))
            return StatusCodes.Status400BadRequest;

        // Default
        return StatusCodes.Status400BadRequest;
    }
}