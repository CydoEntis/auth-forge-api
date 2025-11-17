using System.Net;

namespace AuthForge.Api.Common.Exceptions;

public abstract class ApiException : Exception
{
    public string Code { get; }
    public HttpStatusCode StatusCode { get; }

    protected ApiException(string code, string message, HttpStatusCode statusCode)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}