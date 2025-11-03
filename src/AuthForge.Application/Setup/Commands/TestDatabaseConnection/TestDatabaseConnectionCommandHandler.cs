using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Setup.Commands.TestDatabaseConnection;

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

        if (!isSuccessful)
        {
            _logger.LogWarning("Database connection test failed for {DatabaseType}", command.DatabaseType);
            
            return Result<TestDatabaseConnectionResponse>.Failure(
                SetupErrors.DatabaseConnectionFailed);
        }

        _logger.LogInformation("Database connection test succeeded for {DatabaseType}", command.DatabaseType);
        
        var response = new TestDatabaseConnectionResponse(
            true, 
            "Database connection successful");

        return Result<TestDatabaseConnectionResponse>.Success(response);
    }
}