namespace AuthForge.Application.Common.Interfaces;

public interface IAdminJwtTokenGenerator
{
    string GenerateAccessToken(string email);
    string GenerateRefreshToken();
}