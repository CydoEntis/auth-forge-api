namespace AuthForge.Application.Admin.Common;

public record SetupProgress(
    bool IsDatabaseConfigured,
    bool IsEmailConfigured,
    bool IsAdminCreated);