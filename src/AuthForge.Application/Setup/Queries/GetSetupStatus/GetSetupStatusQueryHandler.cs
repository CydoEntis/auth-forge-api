using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Enums;
using Mediator;

namespace AuthForge.Application.Setup.Queries.GetSetupStatus;

public sealed class GetSetupStatusQueryHandler
    : IQueryHandler<GetSetupStatusQuery, Result<GetSetupStatusResponse>>
{
    private readonly ISetupService _setupService;

    public GetSetupStatusQueryHandler(ISetupService setupService)
    {
        _setupService = setupService;
    }

    public async ValueTask<Result<GetSetupStatusResponse>> Handle(
        GetSetupStatusQuery query,
        CancellationToken cancellationToken)
    {
        var isSetupComplete = await _setupService.IsSetupCompleteAsync();

        if (isSetupComplete)
        {
            var completeResponse = new GetSetupStatusResponse(
                IsSetupRequired: false,
                CurrentStep: SetupStep.Complete,
                Progress: new SetupProgress(
                    IsDatabaseConfigured: true,
                    IsEmailConfigured: true,
                    IsAdminCreated: true));

            return Result<GetSetupStatusResponse>.Success(completeResponse);
        }

        var incompleteResponse = new GetSetupStatusResponse(
            IsSetupRequired: true,
            CurrentStep: SetupStep.DatabaseConfiguration,
            Progress: new SetupProgress(
                IsDatabaseConfigured: false,
                IsEmailConfigured: false,
                IsAdminCreated: false));

        return Result<GetSetupStatusResponse>.Success(incompleteResponse);
    }
}