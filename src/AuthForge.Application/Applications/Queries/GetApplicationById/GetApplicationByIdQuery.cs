using AuthForge.Application.Applications.Models;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Applications.Queries.GetApplicationById;

public sealed record GetApplicationByIdQuery(
    string ApplicationId) : IQuery<Result<ApplicationDetail>>;