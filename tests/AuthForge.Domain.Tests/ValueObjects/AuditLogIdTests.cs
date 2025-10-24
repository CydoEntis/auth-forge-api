using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AuthForge.Domain.Tests.ValueObjects;

public class AuditLogIdTests
{
    [Fact]
    public void CreateUnique_ShouldGenerateNewGuid()
    {
        var id = AuditLogId.CreateUnique();

        id.Should().NotBeNull();
        id.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateUnique_CalledMultipleTimes_ShouldGenerateUniqueGuids()
    {
        var id1 = AuditLogId.CreateUnique();
        var id2 = AuditLogId.CreateUnique();

        id1.Value.Should().NotBe(id2.Value);
    }

    [Fact]
    public void Create_WithValidGuid_ShouldCreateAuditLogId()
    {
        var guid = Guid.NewGuid();

        var id = AuditLogId.Create(guid);

        id.Should().NotBeNull();
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldStillCreate()
    {
        var emptyGuid = Guid.Empty;

        var id = AuditLogId.Create(emptyGuid);

        id.Should().NotBeNull();
        id.Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        var guid = Guid.NewGuid();
        var id1 = AuditLogId.Create(guid);
        var id2 = AuditLogId.Create(guid);

        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        var id1 = AuditLogId.CreateUnique();
        var id2 = AuditLogId.CreateUnique();

        id1.Should().NotBe(id2);
        (id1 == id2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        var guid = Guid.NewGuid();
        var id1 = AuditLogId.Create(guid);
        var id2 = AuditLogId.Create(guid);

        var hash1 = id1.GetHashCode();
        var hash2 = id2.GetHashCode();

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        var guid = Guid.NewGuid();
        var id = AuditLogId.Create(guid);

        var result = id.ToString();

        result.Should().Contain(guid.ToString());
    }
}
