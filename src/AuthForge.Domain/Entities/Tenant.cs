using AuthForge.Domain.Common;
using AuthForge.Domain.Events;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Domain.Entities;

public class Tenant : AggregateRoot<TenantId>
{
    private readonly List<User> _users = new();

    private Tenant()
    {
    }

    private Tenant(TenantId id, string name, string slug, TenantSettings settings) : base(id)
    {
        Name = name;
        Slug = slug;
        Settings = settings;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public TenantSettings Settings { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    public static Tenant Create(string name, string slug, TenantSettings? settings = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Tenant slug cannot be empty.", nameof(slug));

        if (!IsValidSlug(slug))
            throw new ArgumentException("Slug must be lowercase, alphanumeric with hyphens only.", nameof(slug));

        var tenant = new Tenant(TenantId.CreateUnique(), name, slug, settings ?? TenantSettings.Default());

        tenant.RaiseDomainEvent(new TenantEvents.TenantCreatedDomainEvent(tenant.Id, tenant.Name));

        return tenant;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Tenant is already deactivated.");

        IsActive = false;
        DeactivatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new TenantEvents.TenantDeactivatedDomainEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("Tenant is already activate.");

        IsActive = true;
        DeactivatedAtUtc = null;

        RaiseDomainEvent(new TenantEvents.TenantActivatedDomainEvent(Id));
    }

    public void UpdateSettings(TenantSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty.", nameof(name));

        Name = name;
    }

    private static bool IsValidSlug(string slug)
    {
        return slug.All(c => char.IsLower(c) || char.IsDigit(c) || c == '-');
    }
}