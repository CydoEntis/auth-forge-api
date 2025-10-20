using AuthForge.Application.Applications.Models;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Applications.Queries.GetAll;

public sealed record GetApplicationsQuery(
    ApplicationFilterParameters Parameters
) : IQuery<Result<PagedResponse<ApplicationSummary>>>;