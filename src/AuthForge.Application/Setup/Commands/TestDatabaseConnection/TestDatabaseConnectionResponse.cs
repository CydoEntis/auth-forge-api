
namespace AuthForge.Application.Setup.Commands.TestDatabaseConnection;

public record TestDatabaseConnectionResponse(
    bool IsSuccessful,
    string Message);