using AuthForge.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AuthForge.Infrastructure.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Email => _httpContextAccessor.HttpContext?.User
        ?.FindFirst(ClaimTypes.Email)?.Value;

    public string? UserId => _httpContextAccessor.HttpContext?.User
        ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? IpAddress => _httpContextAccessor.HttpContext?
        .Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => _httpContextAccessor.HttpContext?
        .Request.Headers.UserAgent.ToString();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User
        ?.Identity?.IsAuthenticated ?? false;
}