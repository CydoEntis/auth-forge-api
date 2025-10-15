using AuthForge.Domain.Common;
using AuthForge.Domain.Events;
using AuthForge.Domain.ValueObjects;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Domain.Entities;

// An App that a developer would create
public sealed class Application : AggregateRoot<ApplicationId>
{
    private Application()
    {
    }

    private Application(ApplicationId id, AuthForgeUserId userId, string name, string slug) : base(id)
    {
        UserId = userId;
        Name = name;
        Slug = slug;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public AuthForgeUserId UserId { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }
    public ApplicationSettings Settings { get; private set; } = default!;

    public static Application Create(AuthForgeUserId userId, string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Application name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Application slug cannot be empty", nameof(slug));

        var application = new Application(
            ApplicationId.CreateUnique(),
            userId,
            name,
            slug
        );

        application.RaiseDomainEvent(new ApplicationCreatedDomainEvent(application.Id, application.UserId,
            application.Name, application.Slug));

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
        Settings = settings;
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
}