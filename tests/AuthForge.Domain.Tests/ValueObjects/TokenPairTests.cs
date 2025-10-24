using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AuthForge.Domain.Tests.ValueObjects;

public class TokenPairTests
{
    [Fact]
    public void Constructor_WithValidTokens_ShouldCreateTokenPair()
    {
        var accessToken = "access_token_123";
        var refreshToken = "refresh_token_456";
        var accessExpiry = DateTime.UtcNow.AddMinutes(15);
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        var tokenPair = new TokenPair(accessToken, refreshToken, accessExpiry, refreshExpiry);

        tokenPair.Should().NotBeNull();
        tokenPair.AccessToken.Should().Be(accessToken);
        tokenPair.RefreshToken.Should().Be(refreshToken);
        tokenPair.AccessTokenExpiresAt.Should().Be(accessExpiry);
        tokenPair.RefreshTokenExpiresAt.Should().Be(refreshExpiry);
    }

    [Fact]
    public void ExpiresInSeconds_WithFutureExpiry_ShouldReturnPositiveSeconds()
    {
        var futureTime = DateTime.UtcNow.AddMinutes(15);
        var tokenPair = new TokenPair(
            "access_token",
            "refresh_token",
            futureTime,
            DateTime.UtcNow.AddDays(7));

        var expiresIn = tokenPair.ExpiresInSeconds;

        expiresIn.Should().BeGreaterThan(0);
        expiresIn.Should().BeLessThanOrEqualTo(15 * 60); // 15 minutes
    }

    [Fact]
    public void ExpiresInSeconds_WithPastExpiry_ShouldReturnNegativeSeconds()
    {
        var pastTime = DateTime.UtcNow.AddMinutes(-5);
        var tokenPair = new TokenPair(
            "access_token",
            "refresh_token",
            pastTime,
            DateTime.UtcNow.AddDays(7));

        var expiresIn = tokenPair.ExpiresInSeconds;

        expiresIn.Should().BeLessThan(0);
    }

    [Fact]
    public void ExpiresInSeconds_WithNowExpiry_ShouldReturnZeroOrNegative()
    {
        var now = DateTime.UtcNow;
        var tokenPair = new TokenPair(
            "access_token",
            "refresh_token",
            now,
            DateTime.UtcNow.AddDays(7));

        var expiresIn = tokenPair.ExpiresInSeconds;

        expiresIn.Should().BeLessThanOrEqualTo(0);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldBeEqual()
    {
        var accessToken = "access_token";
        var refreshToken = "refresh_token";
        var accessExpiry = DateTime.UtcNow.AddMinutes(15);
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        var tokenPair1 = new TokenPair(accessToken, refreshToken, accessExpiry, refreshExpiry);
        var tokenPair2 = new TokenPair(accessToken, refreshToken, accessExpiry, refreshExpiry);

        tokenPair1.Should().Be(tokenPair2);
    }

    [Fact]
    public void Equals_WithDifferentAccessToken_ShouldNotBeEqual()
    {
        var accessExpiry = DateTime.UtcNow.AddMinutes(15);
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        var tokenPair1 = new TokenPair("token1", "refresh", accessExpiry, refreshExpiry);
        var tokenPair2 = new TokenPair("token2", "refresh", accessExpiry, refreshExpiry);

        tokenPair1.Should().NotBe(tokenPair2);
    }

    [Fact]
    public void Equals_WithDifferentRefreshToken_ShouldNotBeEqual()
    {
        var accessExpiry = DateTime.UtcNow.AddMinutes(15);
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        var tokenPair1 = new TokenPair("access", "refresh1", accessExpiry, refreshExpiry);
        var tokenPair2 = new TokenPair("access", "refresh2", accessExpiry, refreshExpiry);

        tokenPair1.Should().NotBe(tokenPair2);
    }

    [Fact]
    public void Equals_WithDifferentExpiryDates_ShouldNotBeEqual()
    {
        var tokenPair1 = new TokenPair(
            "access",
            "refresh",
            DateTime.UtcNow.AddMinutes(15),
            DateTime.UtcNow.AddDays(7));
        var tokenPair2 = new TokenPair(
            "access",
            "refresh",
            DateTime.UtcNow.AddMinutes(30),
            DateTime.UtcNow.AddDays(7));

        tokenPair1.Should().NotBe(tokenPair2);
    }

    [Fact]
    public void ExpiresInSeconds_CalledMultipleTimes_ShouldDecrease()
    {
        var tokenPair = new TokenPair(
            "access_token",
            "refresh_token",
            DateTime.UtcNow.AddSeconds(10),
            DateTime.UtcNow.AddDays(7));

        var firstCall = tokenPair.ExpiresInSeconds;
        Thread.Sleep(1000); 
        var secondCall = tokenPair.ExpiresInSeconds;

        secondCall.Should().BeLessThan(firstCall);
    }
}
