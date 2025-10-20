using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.EndUsers.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler 
    : IQueryHandler<GetCurrentUserQuery, Result<GetCurrentUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;

    public GetCurrentUserQueryHandler(IEndUserRepository endUserRepository)
    {
        _endUserRepository = endUserRepository;
    }

    public async ValueTask<Result<GetCurrentUserResponse>> Handle(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(query.UserId, cancellationToken);

        if (user == null)
            return Result<GetCurrentUserResponse>.Failure(EndUserErrors.NotFound);

        var response = new GetCurrentUserResponse(
            user.Id.Value.ToString(),
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.IsEmailVerified,
            user.IsActive,
            user.CreatedAtUtc,
            user.LastLoginAtUtc);

        return Result<GetCurrentUserResponse>.Success(response);
    }
}