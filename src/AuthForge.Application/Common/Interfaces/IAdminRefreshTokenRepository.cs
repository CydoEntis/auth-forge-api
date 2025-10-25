using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IAdminRefreshTokenRepository
{
    Task<AdminRefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(AdminRefreshToken refreshToken, CancellationToken cancellationToken = default);
    void Update(AdminRefreshToken refreshToken);
    Task RevokeAllForAdminAsync(AdminId adminId, CancellationToken cancellationToken = default);
}