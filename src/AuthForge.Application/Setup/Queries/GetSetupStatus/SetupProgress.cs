namespace AuthForge.Application.Setup.Queries.GetSetupStatus;

public record SetupProgress(
    bool IsDatabaseConfigured,
    bool IsEmailConfigured,
    bool IsAdminCreated);