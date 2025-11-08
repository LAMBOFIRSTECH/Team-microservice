using System;
using System.Collections.Generic;
using Xunit;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.CoreInterfaces;
using Moq;
namespace Teams.Tests.CORE;
public class AggregateEntityTest
{
    // Test de l'ajout d'un événement de domaine
    [Fact]
    public void AddDomainEvent_ShouldAddEvent()
    {
        // Arrange
        var entity = new TestAggregateEntity(Guid.NewGuid());
        var domainEvent = new Mock<IDomainEvent>().Object;

        // Act
        entity.AddDomainEvent(domainEvent);

        // Assert
        Assert.Single(entity.DomainEvents);
        Assert.Contains(domainEvent, entity.DomainEvents);
    }

    // Test pour ClearDomainEvents
    [Fact]
    public void ClearDomainEvents_ShouldClearAllEvents()
    {
        // Arrange
        var entity = new TestAggregateEntity(Guid.NewGuid());
        var domainEvent = new Mock<IDomainEvent>().Object;
        entity.AddDomainEvent(domainEvent);

        // Act
        entity.ClearDomainEvents();

        // Assert
        Assert.Empty(entity.DomainEvents);
    }

    // Test de la méthode Equals (même Id)
    [Fact]
    public void Equals_ShouldReturnTrue_WhenIdsAreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestAggregateEntity(id);
        var entity2 = new TestAggregateEntity(id);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.True(result);
    }

    // Test de la méthode Equals (Id différent)
    [Fact]
    public void Equals_ShouldReturnFalse_WhenIdsAreDifferent()
    {
        // Arrange
        var entity1 = new TestAggregateEntity(Guid.NewGuid());
        var entity2 = new TestAggregateEntity(Guid.NewGuid());

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.False(result);
    }

    // Test de la méthode GetHashCode
    [Fact]
    public void GetHashCode_ShouldReturnSameHashCode_WhenIdsAreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestAggregateEntity(id);
        var entity2 = new TestAggregateEntity(id);

        // Act
        var hashCode1 = entity1.GetHashCode();
        var hashCode2 = entity2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    // Test de la méthode GetHashCode pour des Id différents
    [Fact]
    public void GetHashCode_ShouldReturnDifferentHashCode_WhenIdsAreDifferent()
    {
        // Arrange
        var entity1 = new TestAggregateEntity(Guid.NewGuid());
        var entity2 = new TestAggregateEntity(Guid.NewGuid());

        // Act
        var hashCode1 = entity1.GetHashCode();
        var hashCode2 = entity2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }
}
public class TestAggregateEntity : AggregateEntity
{
    public TestAggregateEntity(Guid id)
    {
        Id = id;
    }
}

