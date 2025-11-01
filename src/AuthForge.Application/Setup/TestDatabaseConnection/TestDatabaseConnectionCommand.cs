using AuthForge.Domain.Common;
using AuthForge.Domain.Enums;
using Mediator;

namespace AuthForge.Application.Admin.Commands.TestDatabaseConnection;

public record TestDatabaseConnectionCommand(
    DatabaseType DatabaseType,
    string? ConnectionString) : ICommand<Result<TestDatabaseConnectionResponse>>;