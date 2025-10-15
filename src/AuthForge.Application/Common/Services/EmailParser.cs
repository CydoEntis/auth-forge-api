using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Services;

public sealed class EmailParser : IEmailParser
{
    public Result<Email> ParseForRegistration(string emailString)
    {
        try
        {
            var email = Email.Create(emailString);
            return Result<Email>.Success(email);
        }
        catch (ArgumentException)
        {
            return Result<Email>.Failure(ValidationErrors.InvalidEmail());
        }
    }

    public Result<Email> ParseForAuthentication(string emailString)
    {
        try
        {
            var email = Email.Create(emailString);
            return Result<Email>.Success(email);
        }
        catch (ArgumentException)
        {
            return Result<Email>.Failure(ValidationErrors.InvalidCredentials);
        }
    }
}