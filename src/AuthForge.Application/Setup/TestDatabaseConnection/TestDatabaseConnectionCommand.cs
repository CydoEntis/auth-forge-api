using AuthForge.Domain.Common;
using AuthForge.Domain.Enums;
using Mediator;

namespace AuthForge.Application.Setup.TestDatabaseConnection;

public record TestDatabaseConnectionCommand(
    DatabaseType DatabaseType,
    string? ConnectionString) : ICommand<Result<TestDatabaseConnectionResponse>>;