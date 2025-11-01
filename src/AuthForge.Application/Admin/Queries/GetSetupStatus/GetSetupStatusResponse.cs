using AuthForge.Domain.Enums;

namespace AuthForge.Application.Admin.Queries.GetSetupStatus;

public sealed record GetSetupStatusResponse(
    bool IsSetupRequired,
    SetupStep CurrentStep,
    SetupProgress Progress);