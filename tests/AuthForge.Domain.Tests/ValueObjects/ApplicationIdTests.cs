using FluentAssertions;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Domain.Tests.ValueObjects;

public class ApplicationIdTests
{
    [Fact]
    public void CreateUnique_ShouldGenerateNewGuid()
    {
        var id = ApplicationId.CreateUnique();

        id.Should().NotBeNull();
        id.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateUnique_CalledMultipleTimes_ShouldGenerateUniqueGuids()
    {
        var id1 = ApplicationId.CreateUnique();
        var id2 = ApplicationId.CreateUnique();

        id1.Value.Should().NotBe(id2.Value);
    }

    [Fact]
    public void Create_WithValidGuid_ShouldCreateApplicationId()
    {
        var guid = Guid.NewGuid();

        var id = ApplicationId.Create(guid);

        id.Should().NotBeNull();
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldStillCreate()
    {
        var emptyGuid = Guid.Empty;

        var id = ApplicationId.Create(emptyGuid);

        id.Should().NotBeNull();
        id.Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        var guid = Guid.NewGuid();
        var id1 = ApplicationId.Create(guid);
        var id2 = ApplicationId.Create(guid);

        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        var id1 = ApplicationId.CreateUnique();
        var id2 = ApplicationId.CreateUnique();

        id1.Should().NotBe(id2);
        (id1 == id2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        var guid = Guid.NewGuid();
        var id1 = ApplicationId.Create(guid);
        var id2 = ApplicationId.Create(guid);

        var hash1 = id1.GetHashCode();
        var hash2 = id2.GetHashCode();

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        var guid = Guid.NewGuid();
        var id = ApplicationId.Create(guid);

        var result = id.ToString();

        result.Should().Contain(guid.ToString());
    }
}
