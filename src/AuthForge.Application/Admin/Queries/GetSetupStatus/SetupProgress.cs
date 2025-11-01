namespace AuthForge.Application.Admin.Queries.GetSetupStatus;

public record SetupProgress(
    bool IsDatabaseConfigured,
    bool IsEmailConfigured,
    bool IsAdminCreated);