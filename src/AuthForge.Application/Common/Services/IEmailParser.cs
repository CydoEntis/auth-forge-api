using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Services;

public interface IEmailParser
{
    Result<Email> ParseForRegistration(string emailString);
    Result<Email> ParseForAuthentication(string emailString);
}