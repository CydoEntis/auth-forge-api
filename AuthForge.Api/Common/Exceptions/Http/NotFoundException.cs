using System.Net;

namespace AuthForge.Api.Common.Exceptions.Http;

public class NotFoundException : ApiException
{
    public NotFoundException(string message)
        : base(ErrorCodes.NotFound, message, HttpStatusCode.NotFound) { }
}







