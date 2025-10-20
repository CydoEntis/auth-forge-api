using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Queries.GetApplicationUserById;

public record GetApplicationUserByIdQuery(
    ApplicationId ApplicationId,
    EndUserId UserId) : IQuery<Result<GetApplicationUserByIdResponse>>;