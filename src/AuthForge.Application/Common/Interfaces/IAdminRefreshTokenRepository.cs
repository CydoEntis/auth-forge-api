using AuthForge.Domain.Entities;

namespace AuthForge.Application.Common.Interfaces;

public interface IAdminRefreshTokenRepository
{
    Task<AdminRefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(AdminRefreshToken refreshToken, CancellationToken cancellationToken = default);
    void Update(AdminRefreshToken refreshToken);
}