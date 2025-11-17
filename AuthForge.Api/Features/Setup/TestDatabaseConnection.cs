using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Database;
using AuthForge.Api.Features.Setup.Shared.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Setup;

public record TestDatabaseConnectionRequest(DatabaseType DatabaseType, string ConnectionString);

public record TestDatabaseConnectionResponse(bool IsSuccessful, string Message);

public class TestDatabaseConnectionValidator : AbstractValidator<TestDatabaseConnectionRequest>
{
    public TestDatabaseConnectionValidator()
    {
        RuleFor(x => x.DatabaseType)
            .NotNull()
            .WithMessage("Database configuration is required");
        
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("Connection string is required");
    }
}

public class TestDatabaseConnectionHandler
{
    private readonly ILogger<TestDatabaseConnectionHandler> _logger;

    public TestDatabaseConnectionHandler(ILogger<TestDatabaseConnectionHandler> logger)
    {
        _logger = logger;
    }

    public async Task<TestDatabaseConnectionResponse> HandleAsync(
        TestDatabaseConnectionRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Testing database connection: {DatabaseType}", request.DatabaseType);

        var options = BuildDbContextOptions(request.DatabaseType, request.ConnectionString);
        await TestConnectionAsync(options, request.DatabaseType, ct);

        return new TestDatabaseConnectionResponse(true, "Database connection successful");
    }

    private static DbContextOptions BuildDbContextOptions(DatabaseType databaseType, string connectionString)
    {
        return databaseType switch
        {
            DatabaseType.PostgreSql => new DbContextOptionsBuilder<DbContext>()
                .UseNpgsql(connectionString!)
                .Options,
            DatabaseType.Sqlite => new DbContextOptionsBuilder<DbContext>()
                .UseSqlite(connectionString ?? "app.db")
                .Options,
            _ => throw new NotSupportedException($"Database type {databaseType} not supported")
        };
    }

    private async Task TestConnectionAsync(DbContextOptions options, DatabaseType type, CancellationToken ct)
    {
        try
        {
            await using var testDb = new DbContext(options);
            var canConnect = await testDb.Database.CanConnectAsync(ct);
            if (!canConnect)
                throw new DatabaseConnectionException("Cannot connect to the database");

            _logger.LogInformation("Database connection test succeeded for {DatabaseType}", type);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database connection test failed for {DatabaseType}", type);
            throw new DatabaseConnectionException("Database connection test failed: " + ex.Message);
        }
    }
}

public static class TestDatabaseConnectionFeature
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/setup/test-database", async (
                TestDatabaseConnectionRequest request,
                TestDatabaseConnectionHandler handler,
                CancellationToken ct) =>
            {
                var validator = new TestDatabaseConnectionValidator();
                var validationResult = await validator.ValidateAsync(request, ct);
                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<TestDatabaseConnectionResponse>.Ok(response));
            })
            .WithName("TestDatabaseConnection")
            .WithTags("Setup")
            .AllowAnonymous();
    }
}