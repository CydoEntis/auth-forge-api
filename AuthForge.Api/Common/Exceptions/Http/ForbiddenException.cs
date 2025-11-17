using System.Net;

namespace AuthForge.Api.Common.Exceptions.Http
{
    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message)
            : base(ErrorCodes.Unauthorized, message, HttpStatusCode.Forbidden) { }
    }
}