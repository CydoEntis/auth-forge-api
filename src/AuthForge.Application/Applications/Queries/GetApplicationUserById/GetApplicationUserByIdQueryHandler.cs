using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.Applications.Queries.GetApplicationUserById;

public sealed class GetApplicationUserByIdQueryHandler
    : IQueryHandler<GetApplicationUserByIdQuery, Result<GetApplicationUserByIdResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IApplicationRepository _applicationRepository;

    public GetApplicationUserByIdQueryHandler(
        IEndUserRepository endUserRepository,
        IApplicationRepository applicationRepository)
    {
        _endUserRepository = endUserRepository;
        _applicationRepository = applicationRepository;
    }

    public async ValueTask<Result<GetApplicationUserByIdResponse>> Handle(
        GetApplicationUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(
            query.ApplicationId,
            cancellationToken);

        if (application == null)
            return Result<GetApplicationUserByIdResponse>.Failure(ApplicationErrors.NotFound);

        var user = await _endUserRepository.GetByIdAsync(query.UserId, cancellationToken);

        if (user == null)
            return Result<GetApplicationUserByIdResponse>.Failure(EndUserErrors.NotFound);

        if (user.ApplicationId != query.ApplicationId)
            return Result<GetApplicationUserByIdResponse>.Failure(
                new Error("Application.UserNotFound", "User does not belong to this application"));

        var response = new GetApplicationUserByIdResponse(
            user.Id.Value.ToString(),
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.IsEmailVerified,
            user.IsActive,
            user.FailedLoginAttempts,
            user.LockedOutUntil,
            user.CreatedAtUtc,
            user.UpdatedAtUtc,
            user.LastLoginAtUtc);

        return Result<GetApplicationUserByIdResponse>.Success(response);
    }
}