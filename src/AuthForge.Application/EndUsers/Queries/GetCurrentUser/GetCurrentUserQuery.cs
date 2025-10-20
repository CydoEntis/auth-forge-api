using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.EndUsers.Queries.GetCurrentUser;

public record GetCurrentUserQuery(
    EndUserId UserId) : IQuery<Result<GetCurrentUserResponse>>;