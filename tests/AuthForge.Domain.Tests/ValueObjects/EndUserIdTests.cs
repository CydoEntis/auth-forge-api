using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AuthForge.Domain.Tests.ValueObjects;

public class EndUserIdTests
{
    [Fact]
    public void CreateUnique_ShouldGenerateNewGuid()
    {
        var id = EndUserId.CreateUnique();

        id.Should().NotBeNull();
        id.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateUnique_CalledMultipleTimes_ShouldGenerateUniqueGuids()
    {
        var id1 = EndUserId.CreateUnique();
        var id2 = EndUserId.CreateUnique();

        id1.Value.Should().NotBe(id2.Value);
    }

    [Fact]
    public void Create_WithValidGuid_ShouldCreateEndUserId()
    {
        var guid = Guid.NewGuid();

        var id = EndUserId.Create(guid);

        id.Should().NotBeNull();
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldStillCreate()
    {
        var emptyGuid = Guid.Empty;

        var id = EndUserId.Create(emptyGuid);

        id.Should().NotBeNull();
        id.Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        var guid = Guid.NewGuid();
        var id1 = EndUserId.Create(guid);
        var id2 = EndUserId.Create(guid);

        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        var id1 = EndUserId.CreateUnique();
        var id2 = EndUserId.CreateUnique();

        id1.Should().NotBe(id2);
        (id1 == id2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        var guid = Guid.NewGuid();
        var id1 = EndUserId.Create(guid);
        var id2 = EndUserId.Create(guid);

        var hash1 = id1.GetHashCode();
        var hash2 = id2.GetHashCode();

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        var guid = Guid.NewGuid();
        var id = EndUserId.Create(guid);

        var result = id.ToString();

        result.Should().Contain(guid.ToString());
    }
}
