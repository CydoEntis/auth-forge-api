using AuthForge.Application.Applications.Models;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Applications.Queries.GetById;

public sealed record GetApplicationByIdQuery(
    string ApplicationId,
    string UserId) : IQuery<Result<ApplicationDetail>>;