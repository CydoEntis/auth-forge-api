using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace AuthForge.Infrastructure.Data;

public class ConfigurationDatabase
{
    private readonly string _connectionString;
    private readonly ILogger<ConfigurationDatabase> _logger;
    private IDataProtector? _protector;

    // Keys that should be encrypted at rest
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "jwt_secret",
        "smtp_password",
        "resend_api_key",
        "postgres_connection_string"
    };

    public ConfigurationDatabase(string dataDirectory, ILogger<ConfigurationDatabase> logger)
    {
        Directory.CreateDirectory(dataDirectory);
        var dbPath = Path.Combine(dataDirectory, "config.db");
        _connectionString = $"Data Source={dbPath}";
        _logger = logger;

        EnsureCreated();
    }

    /// <summary>
    /// Sets the Data Protection provider for encrypting sensitive configuration values.
    /// Should be called after the service provider is built.
    /// </summary>
    public void SetDataProtector(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector("AuthForge.ConfigurationDatabase");
        _logger.LogInformation("Data protection configured for configuration database");
    }

    private void EnsureCreated()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var createTableCmd = connection.CreateCommand();
        createTableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS settings (
                key TEXT PRIMARY KEY,
                value TEXT NOT NULL,
                updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            )";
        createTableCmd.ExecuteNonQuery();

        var insertDefaultCmd = connection.CreateCommand();
        insertDefaultCmd.CommandText = @"
            INSERT OR IGNORE INTO settings (key, value) 
            VALUES ('setup_complete', 'false')";
        insertDefaultCmd.ExecuteNonQuery();
        
        _logger.LogInformation("Configuration database initialized");
    }

    public async Task<string?> GetAsync(string key)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT value FROM settings WHERE key = @key";
        command.Parameters.AddWithValue("@key", key);

        var result = await command.ExecuteScalarAsync();
        var value = result?.ToString();

        if (value == null)
            return null;

        // Decrypt sensitive values if data protector is available
        if (SensitiveKeys.Contains(key) && _protector != null)
        {
            try
            {
                return _protector.Unprotect(value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to decrypt value for key {Key}. Returning encrypted value.", key);
                return value; // Fallback to encrypted value for backward compatibility
            }
        }

        return value;
    }

    public async Task<bool> GetBoolAsync(string key)
    {
        var value = await GetAsync(key);
        return value?.ToLower() == "true";
    }

    public async Task SetAsync(string key, string? value)
    {
        var valueToStore = value ?? string.Empty;

        // Encrypt sensitive values if data protector is available
        if (!string.IsNullOrEmpty(valueToStore) && SensitiveKeys.Contains(key) && _protector != null)
        {
            try
            {
                valueToStore = _protector.Protect(valueToStore);
                _logger.LogDebug("Encrypted value for sensitive key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to encrypt value for key {Key}. Storing unencrypted.", key);
            }
        }

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO settings (key, value, updated_at)
            VALUES (@key, @value, @updated_at)
            ON CONFLICT(key)
            DO UPDATE SET value = @value, updated_at = @updated_at";

        command.Parameters.AddWithValue("@key", key);
        command.Parameters.AddWithValue("@value", valueToStore);
        command.Parameters.AddWithValue("@updated_at", DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        var settings = new Dictionary<string, string>();
        
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT key, value FROM settings";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            settings[reader.GetString(0)] = reader.GetString(1);
        }

        return settings;
    }
}