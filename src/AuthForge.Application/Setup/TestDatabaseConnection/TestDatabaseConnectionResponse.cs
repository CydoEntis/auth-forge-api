
namespace AuthForge.Application.Setup.TestDatabaseConnection;

public record TestDatabaseConnectionResponse(
    bool IsSuccessful,
    string Message);