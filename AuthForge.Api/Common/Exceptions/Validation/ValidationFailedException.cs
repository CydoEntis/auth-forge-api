using System.Net;

namespace AuthForge.Api.Common.Exceptions.Validation
{
    public class ValidationFailedException : ApiException
    {
        public ValidationFailedException(string message)
            : base(ErrorCodes.ValidationFailed, message, HttpStatusCode.BadRequest) { }
    }
}