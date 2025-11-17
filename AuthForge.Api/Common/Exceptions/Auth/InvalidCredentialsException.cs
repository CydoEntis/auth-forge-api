using System.Net;

namespace AuthForge.Api.Common.Exceptions.Auth;

public class InvalidCredentialsException : ApiException
{
    public InvalidCredentialsException(string message)
        : base(ErrorCodes.InvalidCredentials, message, HttpStatusCode.Unauthorized) { }
}