using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Setup.Queries.GetSetupStatus;

public sealed record GetSetupStatusQuery : IQuery<Result<GetSetupStatusResponse>>;