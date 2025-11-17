using System.Net;

namespace AuthForge.Api.Common.Exceptions;

public class SetupRequiredException : ApiException
{
    public SetupRequiredException()
        : base("Setup.IsRequired",
            "Please complete the setup wizard before using the application",
            HttpStatusCode.ServiceUnavailable)
    {
    }
}