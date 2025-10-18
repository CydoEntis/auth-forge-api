using AuthForge.Application.Common.Models;
using AuthForge.Application.EndUsers.Models;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.EndUsers.Queries.GetAll;

public sealed record GetEndUsersQuery(
    string ApplicationId,
    EndUserFilterParameters Parameters
) : IQuery<Result<PagedResponse<EndUserSummary>>>;