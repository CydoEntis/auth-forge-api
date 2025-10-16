using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;

namespace AuthForge.Domain.Tests.Entities;

public class ApplicationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateApplication()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var name = "My App";
        var slug = "my-app";

        var application = Application.Create(userId, name, slug);

        application.Should().NotBeNull();
        application.UserId.Should().Be(userId);
        application.Name.Should().Be(name);
        application.Slug.Should().Be(slug);
        application.IsActive.Should().BeTrue();
        application.Settings.Should().NotBeNull();
        application.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldInitializeWithDefaultSettings()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());

        var application = Application.Create(userId, "Test App", "test-app");

        application.Settings.MaxFailedLoginAttempts.Should().Be(5);
        application.Settings.LockoutDurationMinutes.Should().Be(15);
        application.Settings.AccessTokenExpirationMinutes.Should().Be(15);
        application.Settings.RefreshTokenExpirationDays.Should().Be(7);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowException(string invalidName)
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());

        Action act = () => Application.Create(userId, invalidName, "valid-slug");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidSlug_ShouldThrowException(string invalidSlug)
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());

        Action act = () => Application.Create(userId, "Valid Name", invalidSlug);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*slug*");
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var application = Application.Create(userId, "Original Name", "original-slug");
        var newName = "Updated Name";

        application.UpdateName(newName);

        application.Name.Should().Be(newName);
        application.UpdatedAtUtc.Should().NotBeNull();
        application.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateName_WithInvalidName_ShouldThrowException(string invalidName)
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var application = Application.Create(userId, "Original Name", "original-slug");

        Action act = () => application.UpdateName(invalidName);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void UpdateSettings_WithValidSettings_ShouldUpdateSettings()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var application = Application.Create(userId, "Test App", "test-app");
        var newSettings = ApplicationSettings.Create(3, 30, 60, 14);

        application.UpdateSettings(newSettings);

        application.Settings.Should().Be(newSettings);
        application.Settings.MaxFailedLoginAttempts.Should().Be(3);
        application.Settings.LockoutDurationMinutes.Should().Be(30);
        application.Settings.AccessTokenExpirationMinutes.Should().Be(60);
        application.Settings.RefreshTokenExpirationDays.Should().Be(14);
        application.UpdatedAtUtc.Should().NotBeNull();
        application.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateApplication()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var application = Application.Create(userId, "Test App", "test-app");

        application.Deactivate();

        application.IsActive.Should().BeFalse();
        application.DeactivatedAtUtc.Should().NotBeNull();
        application.DeactivatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldThrowException()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var application = Application.Create(userId, "Test App", "test-app");
        application.Deactivate(); 

        Action act = () => application.Deactivate(); 

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already deactivated*");
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateApplication()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var application = Application.Create(userId, "Test App", "test-app");
        application.Deactivate();

        application.Activate();

        application.IsActive.Should().BeTrue();
        application.DeactivatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrowException()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var application = Application.Create(userId, "Test App", "test-app");

        Action act = () => application.Activate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already active*");
    }
}