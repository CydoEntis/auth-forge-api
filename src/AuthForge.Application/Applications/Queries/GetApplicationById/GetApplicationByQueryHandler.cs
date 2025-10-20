using AuthForge.Application.Applications.Models;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Queries.GetById;

public sealed class GetApplicationByIdQueryHandler
    : IQueryHandler<GetApplicationByIdQuery, Result<ApplicationDetail>>
{
    private readonly IApplicationRepository _applicationRepository;

    public GetApplicationByIdQueryHandler(IApplicationRepository applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }

    public async ValueTask<Result<ApplicationDetail>> Handle(
        GetApplicationByIdQuery query,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(query.ApplicationId, out var appGuid))
            return Result<ApplicationDetail>.Failure(ValidationErrors.InvalidGuid("ApplicationId"));

        var applicationId = ApplicationId.Create(appGuid);

        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application is null)
            return Result<ApplicationDetail>.Failure(ApplicationErrors.NotFound);

        var applicationSettings = new AppSettings(
            application.Settings.MaxFailedLoginAttempts,
            application.Settings.LockoutDurationMinutes,
            application.Settings.AccessTokenExpirationMinutes,
            application.Settings.RefreshTokenExpirationDays);

        var response = new ApplicationDetail(
            application.Id.Value.ToString(),
            application.Name,
            application.Slug,
            application.PublicKey,
            application.IsActive,
            applicationSettings,
            application.CreatedAtUtc,
            application.UpdatedAtUtc,
            application.DeactivatedAtUtc);

        return Result<ApplicationDetail>.Success(response);
    }
}