using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.EndUsers.Queries.GetById;

public sealed class GetEndUserByIdQueryHandler
    : IQueryHandler<GetEndUserByIdQuery, Result<GetEndUserByIdResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IApplicationRepository _applicationRepository;

    public GetEndUserByIdQueryHandler(
        IEndUserRepository endUserRepository,
        IApplicationRepository applicationRepository)
    {
        _endUserRepository = endUserRepository;
        _applicationRepository = applicationRepository;
    }

    public async ValueTask<Result<GetEndUserByIdResponse>> Handle(
        GetEndUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(
            query.ApplicationId,
            cancellationToken);

        if (application == null)
            return Result<GetEndUserByIdResponse>.Failure(ApplicationErrors.NotFound);

        var user = await _endUserRepository.GetByIdAsync(query.UserId, cancellationToken);

        if (user == null)
            return Result<GetEndUserByIdResponse>.Failure(EndUserErrors.NotFound);

        if (user.ApplicationId != query.ApplicationId)
            return Result<GetEndUserByIdResponse>.Failure(
                new Error("Application.UserNotFound", "User does not belong to this application"));

        var response = new GetEndUserByIdResponse(
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

        return Result<GetEndUserByIdResponse>.Success(response);
    }
}