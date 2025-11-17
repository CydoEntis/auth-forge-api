using System.Net;

namespace AuthForge.Api.Common.Exceptions.Http;

public class ConflictException : ApiException
{
    public ConflictException(string message)
        : base("Conflict", message, HttpStatusCode.Conflict) { }
}