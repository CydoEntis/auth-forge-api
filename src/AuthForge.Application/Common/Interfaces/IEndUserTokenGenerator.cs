using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using App = AuthForge.Domain.Entities.Application;

namespace AuthForge.Application.Common.Interfaces;

public interface IEndUserJwtTokenGenerator
{
    (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(EndUser user, App application);

    TokenPair GenerateTokenPair(
        EndUser user,
        App application,
        string? ipAddress = null,
        string? userAgent = null);

    EndUserId? ValidateToken(string token);
}