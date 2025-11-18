using AuthForge.Api.Features.Shared.Models;

namespace AuthForge.Api.Common.Interfaces;

public interface IJwtService
{
    Task<TokenPair> GenerateAdminTokenPairAsync(Guid adminId, string email);
}