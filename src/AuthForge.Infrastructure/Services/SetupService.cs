using System.Text.Json;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Enums;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
using AuthForge.Infrastructure.Security;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using EmailProvider = AuthForge.Domain.Enums.EmailProvider;

namespace AuthForge.Infrastructure.Services;

public class SetupService : ISetupService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SetupService> _logger;
    private readonly ConfigurationDatabase _configDb;

    public SetupService(
        IConfiguration configuration,
        ILogger<SetupService> logger,
        ConfigurationDatabase configDb)
    {
        _configuration = configuration;
        _logger = logger;
        _configDb = configDb;
    }

    public async Task<bool> IsSetupCompleteAsync()
    {
        return await _configDb.GetBoolAsync("setup_complete");
    }

    public async Task<bool> TestDatabaseConnectionAsync(
        DatabaseConfiguration config,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (config.DatabaseType == DatabaseType.Sqlite)
            {
                _logger.LogInformation("SQLite connection test passed (default)");
                return true;
            }

            if (config.DatabaseType == DatabaseType.PostgreSql)
            {
                if (string.IsNullOrEmpty(config.ConnectionString))
                {
                    _logger.LogWarning("PostgreSQL connection string is empty");
                    return false;
                }

                var optionsBuilder = new DbContextOptionsBuilder<AuthForgeDbContext>();
                optionsBuilder.UseNpgsql(config.ConnectionString);

                await using var testContext = new AuthForgeDbContext(
                    optionsBuilder.Options,
                    new NoOpEncryptionService());
                var canConnect = await testContext.Database.CanConnectAsync(cancellationToken);

                if (canConnect)
                {
                    _logger.LogInformation("PostgreSQL connection test passed");
                    return true;
                }

                _logger.LogWarning("PostgreSQL connection test failed");
                return false;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed");
            return false;
        }
    }

    public async Task<bool> TestEmailConfigurationAsync(
        EmailConfiguration config,
        string testRecipient,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (config.Provider == EmailProvider.Resend)
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ResendApiKey}");

                var emailPayload = new
                {
                    from = $"{config.FromName} <{config.FromEmail}>",
                    to = new[] { testRecipient },
                    subject = "AuthForge Setup - Test Email",
                    html = "<p>This is a test email from AuthForge setup wizard.</p>"
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(emailPayload),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await httpClient.PostAsync(
                    "https://api.resend.com/emails",
                    content,
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Resend test email sent successfully");
                    return true;
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Resend test email failed: {StatusCode} - {Response}",
                    response.StatusCode, responseBody);
                return false;
            }

            if (config.Provider == EmailProvider.Smtp)
            {
                _logger.LogInformation("SMTP Test - Host: {Host}, Port: {Port}, Username: {Username}, UseSsl: {UseSsl}",
                    config.SmtpHost,
                    config.SmtpPort,
                    config.SmtpUsername,
                    config.SmtpUseSsl);

                _logger.LogInformation("SMTP Password Length: {Length}", config.SmtpPassword?.Length ?? 0);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(config.FromName, config.FromEmail));
                message.To.Add(new MailboxAddress("", testRecipient));
                message.Subject = "AuthForge Setup - Test Email";
                message.Body = new TextPart("html")
                {
                    Text = "<p>This is a test email from AuthForge setup wizard.</p>"
                };

                using var client = new MailKit.Net.Smtp.SmtpClient();

                try
                {
                    _logger.LogInformation("Connecting to SMTP server...");

                    await client.ConnectAsync(
                        config.SmtpHost,
                        config.SmtpPort ?? 587,
                        SecureSocketOptions.StartTls,
                        cancellationToken);

                    _logger.LogInformation("Connected. Authenticating...");

                    await client.AuthenticateAsync(
                        config.SmtpUsername,
                        config.SmtpPassword,
                        cancellationToken);

                    _logger.LogInformation("Authenticated. Sending email...");

                    await client.SendAsync(message, cancellationToken);

                    _logger.LogInformation("Email sent. Disconnecting...");

                    await client.DisconnectAsync(true, cancellationToken);

                    _logger.LogInformation("SMTP test email sent successfully using MailKit");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MailKit SMTP test failed. Error: {ErrorMessage}", ex.Message);
                    return false;
                }
            }

            _logger.LogWarning("Unsupported email provider: {Provider}", config.Provider);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email configuration test failed for provider: {Provider}", config.Provider);
            return false;
        }
    }

    public async Task CompleteSetupAsync(
        SetupConfiguration config,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Completing setup...");

        await _configDb.SetAsync("database_type", config.Database.DatabaseType.ToString());

        if (config.Database.DatabaseType == DatabaseType.PostgreSql)
        {
            await _configDb.SetAsync("postgres_connection_string", config.Database.ConnectionString!);
        }

        await _configDb.SetAsync("email_provider", config.Email.Provider.ToString());
        await _configDb.SetAsync("smtp_host", config.Email.SmtpHost);
        await _configDb.SetAsync("smtp_port", config.Email.SmtpPort?.ToString());
        await _configDb.SetAsync("smtp_username", config.Email.SmtpUsername);
        await _configDb.SetAsync("smtp_password", config.Email.SmtpPassword);
        await _configDb.SetAsync("smtp_use_ssl", config.Email.SmtpUseSsl?.ToString());
        await _configDb.SetAsync("resend_api_key", config.Email.ResendApiKey);
        await _configDb.SetAsync("from_email", config.Email.FromEmail);
        await _configDb.SetAsync("from_name", config.Email.FromName);

        var jwtSecret = GenerateJwtSecret();
        await _configDb.SetAsync("jwt_secret", jwtSecret);
        await _configDb.SetAsync("jwt_issuer", "AuthForge");
        await _configDb.SetAsync("jwt_audience", "AuthForgeClient");

        var encryptionKey = GenerateEncryptionKey();
        var encryptionIV = GenerateEncryptionIV();
        await _configDb.SetAsync("encryption_key", encryptionKey);
        await _configDb.SetAsync("encryption_iv", encryptionIV);
        _logger.LogInformation("Generated encryption keys for data protection");

        await InitializeDatabaseAsync(config.Database, cancellationToken);

        await CreateAdminAccountAsync(config.Admin, config.Database, cancellationToken);

        await _configDb.SetAsync("setup_complete", "true");

        _logger.LogInformation("Setup completed successfully");
    }

    private async Task InitializeDatabaseAsync(
        DatabaseConfiguration config,
        CancellationToken cancellationToken)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthForgeDbContext>();

        if (config.DatabaseType == DatabaseType.Sqlite)
        {
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "authforge.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
        else if (config.DatabaseType == DatabaseType.PostgreSql)
        {
            optionsBuilder.UseNpgsql(config.ConnectionString);
        }

        await using var context = new AuthForgeDbContext(
            optionsBuilder.Options,
            new NoOpEncryptionService());
        await context.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Database migrations applied");
    }

    private async Task CreateAdminAccountAsync(
        AdminSetupConfiguration admin,
        DatabaseConfiguration dbConfig,
        CancellationToken cancellationToken)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthForgeDbContext>();

        if (dbConfig.DatabaseType == DatabaseType.Sqlite)
        {
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "authforge.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
        else if (dbConfig.DatabaseType == DatabaseType.PostgreSql)
        {
            optionsBuilder.UseNpgsql(dbConfig.ConnectionString);
        }

        await using var context = new AuthForgeDbContext(
            optionsBuilder.Options,
            new NoOpEncryptionService());

        var email = Email.Create(admin.Email);
        var hashedPassword = HashedPassword.Create(admin.Password);
        var adminEntity = Admin.Create(email, hashedPassword);

        context.Admins.Add(adminEntity);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin account created with email: {Email}", admin.Email);
    }

    private static string GenerateEncryptionKey()
    {
        var key = new byte[32]; // 256 bits for AES-256
        System.Security.Cryptography.RandomNumberGenerator.Fill(key);
        return Convert.ToBase64String(key);
    }

    private static string GenerateEncryptionIV()
    {
        var iv = new byte[16]; // 128 bits
        System.Security.Cryptography.RandomNumberGenerator.Fill(iv);
        return Convert.ToBase64String(iv);
    }

    private static string GenerateJwtSecret()
    {
        // Generate a cryptographically secure random secret (64 bytes = 512 bits)
        var bytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}