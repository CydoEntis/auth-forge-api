using System.Net;

namespace AuthForge.Api.Common.Exceptions.Http;

public class UnauthorizedException : ApiException
{
    public UnauthorizedException(string message)
        : base(ErrorCodes.Unauthorized, message, HttpStatusCode.Unauthorized)
    {
    }
}