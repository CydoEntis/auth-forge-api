using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;

namespace AuthForge.Domain.Tests.Entities;

public class ApplicationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateApplication()
    {
        var name = "My App";
        var slug = "my-app";

        var application = Application.Create(name, slug, null, null);

        application.Should().NotBeNull();
        application.Name.Should().Be(name);
        application.Slug.Should().Be(slug);
        application.IsActive.Should().BeTrue();
        application.Settings.Should().NotBeNull();
        application.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldInitializeWithDefaultSettings()
    {
        var application = Application.Create("Test App", "test-app", null, null);

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
        Action act = () => Application.Create(invalidName, "valid-slug", null, null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidSlug_ShouldThrowException(string invalidSlug)
    {
        Action act = () => Application.Create("Valid Name", invalidSlug, null, null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*slug*");
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        var application = Application.Create("Original Name", "original-slug", null, null);
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
        var application = Application.Create("Original Name", "original-slug", null, null);

        Action act = () => application.UpdateName(invalidName);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void UpdateSettings_WithValidSettings_ShouldUpdateSettings()
    {
        var application = Application.Create("Test App", "test-app", null, null);
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
        var application = Application.Create("Test App", "test-app", null, null);

        application.Deactivate();

        application.IsActive.Should().BeFalse();
        application.DeactivatedAtUtc.Should().NotBeNull();
        application.DeactivatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldThrowException()
    {
        var application = Application.Create("Test App", "test-app", null, null);
        application.Deactivate();

        Action act = () => application.Deactivate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already deactivated*");
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateApplication()
    {
        var application = Application.Create("Test App", "test-app", null, null);
        application.Deactivate();

        application.Activate();

        application.IsActive.Should().BeTrue();
        application.DeactivatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrowException()
    {
        var application = Application.Create("Test App", "test-app", null, null);

        Action act = () => application.Activate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already active*");
    }
}