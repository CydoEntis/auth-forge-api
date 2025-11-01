using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Common;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Setup.TestDatabaseConnection;

public sealed class TestDatabaseConnectionCommandHandler
    : ICommandHandler<TestDatabaseConnectionCommand, Result<TestDatabaseConnectionResponse>>
{
    private readonly ISetupService _setupService;
    private readonly ILogger<TestDatabaseConnectionCommandHandler> _logger;

    public TestDatabaseConnectionCommandHandler(
        ISetupService setupService,
        ILogger<TestDatabaseConnectionCommandHandler> logger)
    {
        _setupService = setupService;
        _logger = logger;
    }

    public async ValueTask<Result<TestDatabaseConnectionResponse>> Handle(
        TestDatabaseConnectionCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Testing database connection: {DatabaseType}", command.DatabaseType);

        var config = new DatabaseConfiguration(
            command.DatabaseType,
            command.ConnectionString);

        var isSuccessful = await _setupService
            .TestDatabaseConnectionAsync(config, cancellationToken);

        var message = isSuccessful
            ? "Database connection successful"
            : "Database connection failed. Please check your configuration.";

        var response = new TestDatabaseConnectionResponse(isSuccessful, message);

        return Result<TestDatabaseConnectionResponse>.Success(response);
    }
}