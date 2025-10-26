using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Admin.Queries.GetCurrentAdmin;

public sealed class GetCurrentAdminQueryHandler 
    : IQueryHandler<GetCurrentAdminQuery, Result<GetCurrentAdminResponse>>
{
    private readonly IAdminRepository _adminRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetCurrentAdminQueryHandler> _logger;

    public GetCurrentAdminQueryHandler(
        IAdminRepository adminRepository,
        ICurrentUserService currentUserService,
        ILogger<GetCurrentAdminQueryHandler> logger)
    {
        _adminRepository = adminRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async ValueTask<Result<GetCurrentAdminResponse>> Handle(
        GetCurrentAdminQuery query,
        CancellationToken cancellationToken)
    {
        var email = _currentUserService.Email;
        
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Attempted to get current admin without authentication");
            return Result<GetCurrentAdminResponse>.Failure(AdminErrors.Unauthorized);
        }

        _logger.LogDebug("Getting current admin profile for {Email}", email);

        var admin = await _adminRepository.GetByEmailAsync(
            Email.Create(email),
            cancellationToken);

        if (admin is null)
        {
            _logger.LogWarning("Admin not found for email {Email}", email);
            return Result<GetCurrentAdminResponse>.Failure(AdminErrors.NotFound);
        }

        var response = new GetCurrentAdminResponse(
            admin.Id.Value,
            admin.Email.Value,
            admin.CreatedAtUtc,
            admin.LastLoginAtUtc,
            admin.FailedLoginAttempts,
            admin.LockedOutUntil);

        _logger.LogInformation("Retrieved current admin profile: {AdminId}", admin.Id);

        return Result<GetCurrentAdminResponse>.Success(response);
    }
}