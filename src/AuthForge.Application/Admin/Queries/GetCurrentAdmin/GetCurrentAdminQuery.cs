using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Admin.Queries.GetCurrentAdmin;

public sealed record GetCurrentAdminQuery : IQuery<Result<GetCurrentAdminResponse>>;