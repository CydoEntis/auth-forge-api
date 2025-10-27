using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Admin.Queries.GetSetupStatus;

public sealed class GetSetupStatusQueryHandler 
    : IQueryHandler<GetSetupStatusQuery, Result<GetSetupStatusResponse>>
{
    private readonly IAdminRepository _adminRepository;
    private readonly ILogger<GetSetupStatusQueryHandler> _logger;

    public GetSetupStatusQueryHandler(
        IAdminRepository adminRepository,
        ILogger<GetSetupStatusQueryHandler> logger)
    {
        _adminRepository = adminRepository;
        _logger = logger;
    }

    public async ValueTask<Result<GetSetupStatusResponse>> Handle(
        GetSetupStatusQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking admin setup status");

        var adminExists = await _adminRepository.AnyExistsAsync(cancellationToken);
        
        var isSetupRequired = !adminExists;
        
        _logger.LogInformation("Setup status check: IsSetupRequired={IsSetupRequired}", isSetupRequired);

        var response = new GetSetupStatusResponse(isSetupRequired);
        
        return Result<GetSetupStatusResponse>.Success(response);
    }
}