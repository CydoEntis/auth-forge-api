using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.EndUsers.Queries.GetById;

public record GetEndUserByIdQuery(
    ApplicationId ApplicationId,
    EndUserId UserId) : IQuery<Result<GetEndUserByIdResponse>>;