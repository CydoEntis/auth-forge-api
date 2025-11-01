using System.Text.Json;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Enums;
using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Data;
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

    public SetupService(
        IConfiguration configuration,
        ILogger<SetupService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<bool> IsSetupCompleteAsync()
    {
        var isComplete = _configuration.GetValue<bool>("Setup:IsComplete");
        return Task.FromResult(isComplete);
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

                await using var testContext = new AuthForgeDbContext(optionsBuilder.Options);
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

        await WriteConfigurationAsync(config);

        await InitializeDatabaseAsync(config.Database, cancellationToken);

        await CreateAdminAccountAsync(config.Admin, config.Database, cancellationToken);

        _logger.LogInformation("Setup completed successfully");
    }

    private async Task WriteConfigurationAsync(SetupConfiguration config)
    {
        var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        var existingJson = await File.ReadAllTextAsync(appSettingsPath);
        using var document = JsonDocument.Parse(existingJson);
        var root = document.RootElement;

        var updatedSettings = new Dictionary<string, object>();

        if (root.TryGetProperty("Serilog", out var serilog))
            updatedSettings["Serilog"] = JsonSerializer.Deserialize<object>(serilog.GetRawText())!;

        if (root.TryGetProperty("AllowedHosts", out var allowedHosts))
            updatedSettings["AllowedHosts"] = allowedHosts.GetString()!;

        if (root.TryGetProperty("Cors", out var cors))
            updatedSettings["Cors"] = JsonSerializer.Deserialize<object>(cors.GetRawText())!;

        updatedSettings["DatabaseProvider"] = config.Database.DatabaseType.ToString();

        updatedSettings["ConnectionStrings"] = new Dictionary<string, string>
        {
            ["DefaultConnection"] = config.Database.DatabaseType == DatabaseType.Sqlite
                ? ""
                : config.Database.ConnectionString ?? ""
        };

        updatedSettings["Setup"] = new Dictionary<string, bool>
        {
            ["IsComplete"] = true
        };

        var authForgeSection = new Dictionary<string, object>
        {
            ["Jwt"] = new Dictionary<string, object>
            {
                ["Secret"] = GenerateJwtSecret(),
                ["Issuer"] = "AuthForge",
                ["Audience"] = "AuthForgeClient",
                ["AccessTokenExpirationMinutes"] = 15,
                ["RefreshTokenExpirationDays"] = 7
            },
            ["Email"] = new Dictionary<string, object>
            {
                ["Provider"] = config.Email.Provider.ToString(),
                ["ResendApiKey"] = config.Email.ResendApiKey ?? "",
                ["SmtpHost"] = config.Email.SmtpHost ?? "",
                ["SmtpPort"] = config.Email.SmtpPort ?? 0,
                ["SmtpUsername"] = config.Email.SmtpUsername ?? "",
                ["SmtpPassword"] = config.Email.SmtpPassword ?? "",
                ["SmtpUseSsl"] = config.Email.SmtpUseSsl ?? false,
                ["FromEmail"] = config.Email.FromEmail,
                ["FromName"] = config.Email.FromName,
                ["AdminResetCallbackUrl"] = "http://localhost:5000/admin/reset-password"
            },
            ["OAuth"] = new Dictionary<string, object>
            {
                ["Google"] = new Dictionary<string, string>
                {
                    ["ClientId"] = "",
                    ["ClientSecret"] = ""
                },
                ["GitHub"] = new Dictionary<string, string>
                {
                    ["ClientId"] = "",
                    ["ClientSecret"] = ""
                }
            }
        };

        updatedSettings["AuthForge"] = authForgeSection;

        var json = JsonSerializer.Serialize(updatedSettings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(appSettingsPath, json);
        _logger.LogInformation("Configuration written to appsettings.json");
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

        await using var context = new AuthForgeDbContext(optionsBuilder.Options);
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

        await using var context = new AuthForgeDbContext(optionsBuilder.Options);

        var email = Email.Create(admin.Email);
        var hashedPassword = HashedPassword.Create(admin.Password);
        var adminEntity = Admin.Create(email, hashedPassword);

        context.Admins.Add(adminEntity);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin account created with email: {Email}", admin.Email);
    }

    private static string GenerateJwtSecret()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 64)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}