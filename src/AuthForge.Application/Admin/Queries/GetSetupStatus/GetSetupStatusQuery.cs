using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Admin.Queries.GetSetupStatus;

public sealed record GetSetupStatusQuery : IQuery<Result<GetSetupStatusResponse>>;