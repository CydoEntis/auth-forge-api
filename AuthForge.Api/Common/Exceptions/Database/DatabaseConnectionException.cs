using System.Net;

namespace AuthForge.Api.Common.Exceptions.Database;

public class DatabaseConnectionException : ApiException
{
    public DatabaseConnectionException(string message)
        : base(ErrorCodes.DatabaseConnectionFailed, message, HttpStatusCode.ServiceUnavailable)
    {
    }
}