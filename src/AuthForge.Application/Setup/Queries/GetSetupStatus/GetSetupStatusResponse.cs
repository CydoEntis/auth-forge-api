using AuthForge.Domain.Enums;

namespace AuthForge.Application.Setup.Queries.GetSetupStatus;

public sealed record GetSetupStatusResponse(
    bool IsSetupRequired,
    SetupStep CurrentStep,
    SetupProgress Progress);