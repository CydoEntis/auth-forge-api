using AuthForge.Domain.Common;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Queries.GetApplicationKeys;

public record GetApplicationKeysQuery(ApplicationId ApplicationId) 
    : IQuery<Result<GetApplicationKeysResponse>>;

