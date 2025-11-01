
namespace AuthForge.Application.Admin.Commands.TestDatabaseConnection;

public record TestDatabaseConnectionResponse(
    bool IsSuccessful,
    string Message);