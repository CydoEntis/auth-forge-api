using System.Net;

namespace AuthForge.Api.Common.Exceptions.Http;



public class BadRequestException : ApiException
{
    public BadRequestException(string message)
        : base(ErrorCodes.BadRequest, message, HttpStatusCode.BadRequest)
    {
    }
}