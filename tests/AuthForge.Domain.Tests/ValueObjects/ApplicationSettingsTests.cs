using AuthForge.Domain.ValueObjects;
using FluentAssertions;

namespace AuthForge.Domain.Tests.ValueObjects;

public class ApplicationSettingsTests
{
    [Fact]
    public void Default_ShouldReturnSettingsWithExpectedValue()
    {
        var settings = ApplicationSettings.Default();

        settings.MaxFailedLoginAttempts.Should().Be(5);
        settings.LockoutDurationMinutes.Should().Be(15);
        settings.AccessTokenExpirationMinutes.Should().Be(15);
        settings.RefreshTokenExpirationDays.Should().Be(7);
    }

    [Fact]
    public void Create_WithValidValues_ShouldSucceed()
    {
        int maxFailedLoginAttempts = 3;
        int lockoutDurationMinutes = 30;
        int accessTokenExpirationMinutes = 60;
        int refreshTokenExpirationDays = 14;

        var settings = ApplicationSettings.Create(maxFailedLoginAttempts, lockoutDurationMinutes,
            accessTokenExpirationMinutes, refreshTokenExpirationDays);

        settings.MaxFailedLoginAttempts.Should().Be(maxFailedLoginAttempts);
        settings.LockoutDurationMinutes.Should().Be(lockoutDurationMinutes);
        settings.AccessTokenExpirationMinutes.Should().Be(accessTokenExpirationMinutes);
        settings.RefreshTokenExpirationDays.Should().Be(refreshTokenExpirationDays);
    }

    [Theory]
    [InlineData(0, 15, 15, 7)]
    [InlineData(-1, 15, 15, 7)]
    [InlineData(11, 15, 15, 7)]
    public void Create_WithInvalidMaxFailedLoginAttempts_ShouldThrowException(int maxFailedLoginAttempts,
        int lockoutDurationMinutes, int accessTokenExpirationMinutes, int refreshTokenExpirationDays)
    {
        Action act = () => ApplicationSettings.Create(maxFailedLoginAttempts, lockoutDurationMinutes,
            accessTokenExpirationMinutes, refreshTokenExpirationDays);

        act.Should().Throw<ArgumentException>().WithMessage("*Max failed login attempts*");
    }
    
    [Theory]
    [InlineData(5, 0, 15, 7)]
    [InlineData(5, -1, 15, 7)]
    [InlineData(5, 1441, 15, 7)]
    public void Create_WithInvalidLockoutDuration_ShouldThrowException(
        int maxFailedAttempts, int lockoutDuration, int accessTokenExpiration, int refreshTokenExpiration)
    {
        Action act = () => ApplicationSettings.Create(
            maxFailedAttempts,
            lockoutDuration,
            accessTokenExpiration,
            refreshTokenExpiration);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Lockout duration*");
    }

    [Theory]
    [InlineData(5, 15, 0, 7)]
    [InlineData(5, 15, -1, 7)]
    [InlineData(5, 15, 1441, 7)]
    public void Create_WithInvalidAccessTokenExpiration_ShouldThrowException(
        int maxFailedAttempts, int lockoutDuration, int accessTokenExpiration, int refreshTokenExpiration)
    {
        Action act = () => ApplicationSettings.Create(
            maxFailedAttempts,
            lockoutDuration,
            accessTokenExpiration,
            refreshTokenExpiration);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Access token expiration*");
    }

    [Theory]
    [InlineData(5, 15, 15, 0)]
    [InlineData(5, 15, 15, -1)]
    [InlineData(5, 15, 15, 91)]
    public void Create_WithInvalidRefreshTokenExpiration_ShouldThrowException(
        int maxFailedAttempts, int lockoutDuration, int accessTokenExpiration, int refreshTokenExpiration)
    {
        Action act = () => ApplicationSettings.Create(
            maxFailedAttempts,
            lockoutDuration,
            accessTokenExpiration,
            refreshTokenExpiration);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Refresh token expiration*");
    }
}