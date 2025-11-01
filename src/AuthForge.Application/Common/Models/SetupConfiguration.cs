namespace AuthForge.Application.Common.Models;

public record SetupConfiguration(
    DatabaseConfiguration Database,
    EmailConfiguration Email,
    AdminSetupConfiguration Admin);
