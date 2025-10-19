using AuthForge.Domain.Common;
using AuthForge.Domain.Events;
using AuthForge.Domain.ValueObjects;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Domain.Entities;

/**
 * Represents an application created by the admin for end-user authentication.
 * In self-hosted mode, there is only one admin who manages all applications.
 */
public sealed class Application : AggregateRoot<ApplicationId>
{
    private Application()
    {
    }

    private Application(ApplicationId id, string name, string slug, string publicKey, string secretKey) : base(id)
    {
        Name = name;
        Slug = slug;
        PublicKey = publicKey;
        SecretKey = secretKey;
        IsActive = true;
        Settings = ApplicationSettings.Default();
        CreatedAtUtc = DateTime.UtcNow;
    }

    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string PublicKey { get; private set; } = string.Empty;
    public string SecretKey { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }
    public ApplicationSettings Settings { get; private set; } = default!;
    public ApplicationEmailSettings? ApplicationEmailSettings { get; private set; }

    private readonly List<string> _allowedOrigins = new();
    public IReadOnlyList<string> AllowedOrigins => _allowedOrigins.AsReadOnly();

    public static Application Create(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Application name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Application slug cannot be empty", nameof(slug));

        var publicKey = GeneratePublicKey();
        var secretKey = GenerateSecretKey();


        var application = new Application(
            ApplicationId.CreateUnique(),
            name,
            slug,
            publicKey,
            secretKey
        );

        application.RaiseDomainEvent(new ApplicationCreatedDomainEvent(
            application.Id,
            application.Name,
            application.Slug));

        return application;
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
    }

    public void RegenerateKeys()
    {
        PublicKey = GeneratePublicKey();
        SecretKey = GenerateSecretKey();
        UpdatedAtUtc = DateTime.UtcNow;
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
}