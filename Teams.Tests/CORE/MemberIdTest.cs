using System;
using Xunit;
using  Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;

namespace Teams.Tests.CORE;

public class MemberIdTest
{
    [Fact]
    public void Constructor_ShouldCreateValidMemberId_WhenGuidIsNotEmpty()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var memberId = new MemberId(guid);

        // Assert
        Assert.Equal(guid, memberId.Value);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenGuidIsEmpty()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new MemberId(emptyGuid));
        Assert.Equal("MemberId cannot be empty. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenMemberIdsAreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var memberId1 = new MemberId(guid);
        var memberId2 = new MemberId(guid);

        // Act
        var result = memberId1.Equals(memberId2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenMemberIdsAreNotEqual()
    {
        // Arrange
        var memberId1 = new MemberId(Guid.NewGuid());
        var memberId2 = new MemberId(Guid.NewGuid());

        // Act
        var result = memberId1.Equals(memberId2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorEquals_ShouldReturnTrue_WhenMemberIdsAreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var memberId1 = new MemberId(guid);
        var memberId2 = new MemberId(guid);

        // Act
        var result = memberId1 == memberId2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEquals_ShouldReturnFalse_WhenMemberIdsAreNotEqual()
    {
        // Arrange
        var memberId1 = new MemberId(Guid.NewGuid());
        var memberId2 = new MemberId(Guid.NewGuid());

        // Act
        var result = memberId1 == memberId2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEquals_ShouldReturnTrue_WhenMemberIdsAreNotEqual()
    {
        // Arrange
        var memberId1 = new MemberId(Guid.NewGuid());
        var memberId2 = new MemberId(Guid.NewGuid());

        // Act
        var result = memberId1 != memberId2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorNotEquals_ShouldReturnFalse_WhenMemberIdsAreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var memberId1 = new MemberId(guid);
        var memberId2 = new MemberId(guid);

        // Act
        var result = memberId1 != memberId2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_ShouldReturnSameHashCode_ForEqualMemberIds()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var memberId1 = new MemberId(guid);
        var memberId2 = new MemberId(guid);

        // Act
        var hashCode1 = memberId1.GetHashCode();
        var hashCode2 = memberId2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void ToString_ShouldReturnStringRepresentationOfGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var memberId = new MemberId(guid);

        // Act
        var result = memberId.ToString();

        // Assert
        Assert.Equal(guid.ToString(), result);
    }
}
