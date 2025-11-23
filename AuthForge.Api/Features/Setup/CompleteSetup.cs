using System.Security.Cryptography;
using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Database;
using AuthForge.Api.Features.Setup.Shared.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Entities;
using AuthForge.Api.Features.Shared.Models;
using AuthForge.Api.Features.Shared.Validators;
using Microsoft.AspNetCore.Identity;

namespace AuthForge.Api.Features.Setup;

public record AdminCredentials(string Email, string Password, string ConfirmPassword);

public record CompleteSetupRequest(
    string AuthForgeDomain,
    DatabaseType DatabaseType,
    string ConnectionString,
    EmailProviderConfig EmailProviderConfig,
    AdminCredentials AdminCredentials);

public record CompleteSetupResponse(string Message);

public class AdminCredentialsValidator : AbstractValidator<AdminCredentials>
{
    public AdminCredentialsValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid admin email is required");
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Must contain uppercase")
            .Matches(@"[a-z]").WithMessage("Must contain lowercase")
            .Matches(@"[0-9]").WithMessage("Must contain a number")
            .Matches(@"[\W_]").WithMessage("Must contain special character");
    }
}

public class CompleteSetupValidator : AbstractValidator<CompleteSetupRequest>
{
    public CompleteSetupValidator()
    {
        RuleFor(x => x.AuthForgeDomain)
            .NotEmpty()
            .WithMessage("AuthForge domain is required")
            .Must(BeValidUrl)
            .WithMessage("Must be a valid URL (e.g., https://auth.mycompany.com)");

        RuleFor(x => x.DatabaseType)
            .IsInEnum()
            .WithMessage("Invalid database type");

        When(x => x.DatabaseType == DatabaseType.PostgreSql, () =>
        {
            RuleFor(x => x.ConnectionString)
                .NotEmpty()
                .WithMessage("Connection string is required for PostgreSQL")
                .WithErrorCode("Validation.ConnectionString");
        });

        RuleFor(x => x.EmailProviderConfig).NotNull().SetValidator(new EmailProviderConfigValidator());
        RuleFor(x => x.AdminCredentials).NotNull().SetValidator(new AdminCredentialsValidator());
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

public class CompleteSetupHandler
{
    private readonly ConfigDbContext _configDb;
    private readonly ILogger<CompleteSetupHandler> _logger;
    private readonly IEncryptionService _encryptionService;
    private readonly PasswordHasher<Entities.Admin> _passwordHasher;
    private readonly IWebHostEnvironment _environment;

    public CompleteSetupHandler(
        ConfigDbContext configDb,
        ILogger<CompleteSetupHandler> logger,
        IEncryptionService encryptionService,
        PasswordHasher<Entities.Admin> passwordHasher,
        IWebHostEnvironment environment)
    {
        _configDb = configDb;
        _logger = logger;
        _encryptionService = encryptionService;
        _passwordHasher = passwordHasher;
        _environment = environment;
    }

    public async Task<CompleteSetupResponse> HandleAsync(CompleteSetupRequest request, CancellationToken ct)
    {
        var existingConfig = await _configDb.Configuration.FirstOrDefaultAsync(ct);

        if (existingConfig?.IsSetupComplete == true)
            throw new ConflictException("Setup has already been completed.");

        await SaveConfigAsync(request, ct);
        await InitializeMainDatabaseAsync(request, ct);

        _logger.LogInformation("Setup completed successfully");
        return new CompleteSetupResponse("Setup completed successfully.");
    }

    private async Task SaveConfigAsync(CompleteSetupRequest request, CancellationToken ct)
    {
        var config = await _configDb.Configuration.FirstOrDefaultAsync(ct);

        if (config == null)
        {
            config = new Configuration();
            _configDb.Configuration.Add(config);
        }

        try
        {
            string connectionString;
            if (request.DatabaseType == DatabaseType.Sqlite)
            {
                var dbPath = Path.Combine(_environment.ContentRootPath, "Data", "Databases", "authforge.db");
                
                var directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                connectionString = $"Data Source={dbPath}";
                _logger.LogInformation("SQLite database will be created at: {Path}", dbPath);
            }
            else
            {
                connectionString = request.ConnectionString;
            }

            var jwtSecret = GenerateSecureSecret(64);
            _logger.LogInformation("Generated new JWT secret for admin authentication");

            config.IsSetupComplete = true;
            
            config.AuthForgeDomain = request.AuthForgeDomain.TrimEnd('/');
            
            config.DatabaseProvider = request.DatabaseType.ToString();
            config.DatabaseConnectionString = _encryptionService.Encrypt(connectionString);

            config.JwtSecretEncrypted = _encryptionService.Encrypt(jwtSecret);

            config.EmailProvider = request.EmailProviderConfig.EmailProvider.ToString();
            config.EmailFromAddress = request.EmailProviderConfig.FromEmail;
            config.EmailFromName = request.EmailProviderConfig.FromName;
            config.SmtpHost = request.EmailProviderConfig.SmtpHost;
            config.SmtpPort = request.EmailProviderConfig.SmtpPort;
            config.SmtpUsername = request.EmailProviderConfig.SmtpUsername;
            config.SmtpPasswordEncrypted = !string.IsNullOrEmpty(request.EmailProviderConfig.SmtpPassword)
                ? _encryptionService.Encrypt(request.EmailProviderConfig.SmtpPassword)
                : null;
            config.SmtpUseSsl = true;
            config.ResendApiKeyEncrypted = !string.IsNullOrEmpty(request.EmailProviderConfig.ResendApiKey)
                ? _encryptionService.Encrypt(request.EmailProviderConfig.ResendApiKey)
                : null;

            await _configDb.SaveChangesAsync(ct);

            _logger.LogInformation("Configuration saved successfully with JWT secret");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save setup configuration");
            throw new DatabaseConnectionException("Failed to save setup configuration.");
        }
    }

    private async Task InitializeMainDatabaseAsync(CompleteSetupRequest request, CancellationToken ct)
    {
        string connectionString;

        if (request.DatabaseType == DatabaseType.Sqlite)
        {
            var dbPath = Path.Combine(_environment.ContentRootPath, "Data", "Databases", "authforge.db");
            
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            connectionString = $"Data Source={dbPath}";
            _logger.LogInformation("Using SQLite database at: {Path}", dbPath);
        }
        else
        {
            connectionString = request.ConnectionString;
        }

        var options = request.DatabaseType switch
        {
            DatabaseType.PostgreSql => new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options,

            DatabaseType.Sqlite => new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connectionString)
                .Options,

            DatabaseType.SqlServer => new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options,

            DatabaseType.MySql => new DbContextOptionsBuilder<AppDbContext>()
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                .Options,

            _ => throw new NotSupportedException($"Database type {request.DatabaseType} is not supported")
        };

        try
        {
            using var appDb = new AppDbContext(options);

            _logger.LogInformation("Running migrations on main database...");
            await appDb.Database.MigrateAsync(ct);
            _logger.LogInformation("Migrations completed successfully");

            if (!await appDb.Admins.AnyAsync(ct))
            {
                _logger.LogInformation("Creating admin user...");
                appDb.Admins.Add(new Entities.Admin
                {
                    Email = request.AdminCredentials.Email,
                    PasswordHash = _passwordHasher.HashPassword(null!, request.AdminCredentials.Password),
                    CreatedAtUtc = DateTime.UtcNow
                });
                await appDb.SaveChangesAsync(ct);
                _logger.LogInformation("Admin user created: {Email}", request.AdminCredentials.Email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize main database. Connection string: {ConnectionString}",
                connectionString);
            throw new DatabaseConnectionException("Failed to initialize main application database.");
        }
    }

    private static string GenerateSecureSecret(int byteLength = 64)
    {
        var randomBytes = new byte[byteLength];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

public static class CompleteSetupFeature
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/setup/complete", async (
                CompleteSetupRequest request,
                CompleteSetupHandler handler,
                CancellationToken ct) =>
            {
                var validator = new CompleteSetupValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<CompleteSetupResponse>.Ok(response));
            })
            .WithName("CompleteSetup")
            .WithTags("Setup")
            .AllowAnonymous();
    }
}