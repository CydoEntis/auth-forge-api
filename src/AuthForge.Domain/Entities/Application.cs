using AuthForge.Domain.Common;
using AuthForge.Domain.Events;
using AuthForge.Domain.ValueObjects;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Domain.Entities;

public sealed class Application : AggregateRoot<ApplicationId>
{
    private readonly List<string> _allowedOrigins = new();

    private Application()
    {
    }

    private Application(
        ApplicationId id,
        string name,
        string slug,
        string? description,
        string publicKey,
        string secretKey,
        string jwtSecret,
        List<string>? allowedOrigins) : base(id)
    {
        Name = name;
        Slug = slug;
        Description = description;
        PublicKey = publicKey;
        SecretKey = secretKey;
        JwtSecret = jwtSecret;
        IsActive = true;
        Settings = ApplicationSettings.Default();
        CreatedAtUtc = DateTime.UtcNow;

        if (allowedOrigins?.Any() == true)
        {
            foreach (var origin in allowedOrigins)
            {
                AddAllowedOrigin(origin);
            }
        }
    }

    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public string PublicKey { get; private set; } = string.Empty;
    public string SecretKey { get; private set; } = string.Empty;
    public string JwtSecret { get; private set; } = string.Empty; // ✅ NEW
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }

    public ApplicationSettings Settings { get; private set; } = default!;
    public ApplicationEmailSettings? ApplicationEmailSettings { get; private set; }
    public OAuthSettings? OAuthSettings { get; private set; } // ✅ NEW

    public IReadOnlyList<string> AllowedOrigins => _allowedOrigins.AsReadOnly();

    public static Application Create(
        string name,
        string slug,
        string? description,
        List<string>? allowedOrigins)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Application name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Application slug cannot be empty", nameof(slug));

        if (allowedOrigins?.Any() == true)
        {
            foreach (var origin in allowedOrigins)
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out _))
                    throw new ArgumentException($"Invalid origin URL: {origin}", nameof(allowedOrigins));
            }
        }

        var publicKey = GeneratePublicKey();
        var secretKey = GenerateSecretKey();
        var jwtSecret = GenerateJwtSecret();

        var application = new Application(
            ApplicationId.CreateUnique(),
            name,
            slug,
            description,
            publicKey,
            secretKey,
            jwtSecret,
            allowedOrigins
        );

        application.RaiseDomainEvent(new ApplicationCreatedDomainEvent(
            application.Id,
            application.Name,
            application.Slug));

        return application;
    }

    public void Update(
        string? name = null,
        string? description = null,
        bool? isActive = null,
        List<string>? allowedOrigins = null)
    {
        if (name != null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Application name cannot be empty", nameof(name));
            Name = name;
        }

        if (description != null)
        {
            Description = description;
        }

        if (isActive.HasValue && isActive.Value != IsActive)
        {
            if (isActive.Value)
                Activate();
            else
                Deactivate();
        }

        if (allowedOrigins != null)
        {
            foreach (var origin in allowedOrigins)
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out _))
                    throw new ArgumentException($"Invalid origin URL: {origin}", nameof(allowedOrigins));
            }

            _allowedOrigins.Clear();
            foreach (var origin in allowedOrigins)
            {
                _allowedOrigins.Add(origin);
            }
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Application name cannot be empty", nameof(name));

        Name = name;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateSettings(ApplicationSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Application is already deactivated.");

        IsActive = false;
        DeactivatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new ApplicationDeactivatedDomainEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("Application is already active.");

        IsActive = true;
        DeactivatedAtUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void RegenerateKeys()
    {
        PublicKey = GeneratePublicKey();
        SecretKey = GenerateSecretKey();
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new ApplicationKeysRegeneratedDomainEvent(Id, DateTime.UtcNow));
    }

    public void RegenerateJwtSecret()
    {
        JwtSecret = GenerateJwtSecret();
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new JwtSecretRegeneratedDomainEvent(Id, DateTime.UtcNow));
    }

    public void AddAllowedOrigin(string origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
            throw new ArgumentException("Origin cannot be empty", nameof(origin));

        if (!Uri.TryCreate(origin, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid origin URL", nameof(origin));

        if (!_allowedOrigins.Contains(origin))
        {
            _allowedOrigins.Add(origin);
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public void UpdateAllowedOrigin(string oldOrigin, string newOrigin)
    {
        if (string.IsNullOrWhiteSpace(oldOrigin))
            throw new ArgumentException("Old origin cannot be empty", nameof(oldOrigin));

        if (string.IsNullOrWhiteSpace(newOrigin))
            throw new ArgumentException("New origin cannot be empty", nameof(newOrigin));

        if (!Uri.TryCreate(newOrigin, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid new origin URL", nameof(newOrigin));

        if (!_allowedOrigins.Contains(oldOrigin))
            throw new InvalidOperationException($"Origin '{oldOrigin}' does not exist");

        if (_allowedOrigins.Contains(newOrigin))
            throw new InvalidOperationException($"Origin '{newOrigin}' already exists");

        var index = _allowedOrigins.IndexOf(oldOrigin);
        _allowedOrigins[index] = newOrigin;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void RemoveAllowedOrigin(string origin)
    {
        if (_allowedOrigins.Remove(origin))
        {
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public void ClearAllowedOrigins()
    {
        _allowedOrigins.Clear();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ConfigureEmail(ApplicationEmailSettings settings)
    {
        ApplicationEmailSettings = settings ?? throw new ArgumentNullException(nameof(settings));
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void RemoveEmailConfiguration()
    {
        ApplicationEmailSettings = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ConfigureOAuth(OAuthSettings settings)
    {
        OAuthSettings = settings ?? throw new ArgumentNullException(nameof(settings));
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void RemoveOAuthConfiguration()
    {
        OAuthSettings = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string GeneratePublicKey()
    {
        var random = Guid.NewGuid().ToString("N");
        return $"pk_live_{random}";
    }

    private static string GenerateSecretKey()
    {
        var random = Guid.NewGuid().ToString("N");
        return $"sk_live_{random}";
    }

    private static string GenerateJwtSecret()
    {
        var bytes = new byte[64]; // 512 bits
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}