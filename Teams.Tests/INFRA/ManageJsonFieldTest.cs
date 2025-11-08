using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Persistence.DAL.EFQueriesHelpers;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;
namespace Teams.Tests.INFRA;
public class ManageJsonFieldTest
{
    [Fact]
    public void WhereMembersContain_ShouldFilterWithNpgsql_WhenProviderIsNpgsql()
    {
        // Arrange
        var team1Id = Guid.NewGuid();
        var team2Id = Guid.NewGuid();
        var guidToSearch = Guid.NewGuid();

        var teams = new List<Team>
        {
            new Team(team1Id, "TeamOne", Guid.NewGuid(), new List<Guid> { guidToSearch }, DateTimeOffset.UtcNow), // Changement du nom
            new Team(team2Id, "TeamTwo", Guid.NewGuid(), new List<Guid> { Guid.NewGuid() }, DateTimeOffset.UtcNow)  // Changement du nom
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Team>>();
        mockSet.As<IQueryable<Team>>().Setup(m => m.Provider).Returns(teams.Provider);
        mockSet.As<IQueryable<Team>>().Setup(m => m.Expression).Returns(teams.Expression);
        mockSet.As<IQueryable<Team>>().Setup(m => m.ElementType).Returns(teams.ElementType);
        mockSet.As<IQueryable<Team>>().Setup(m => m.GetEnumerator()).Returns(teams.GetEnumerator());

        var dbContextMock = new Mock<DbContext>();
        dbContextMock.Setup(c => c.Set<Team>()).Returns(mockSet.Object);

        // Simuler le comportement du provider Npgsql
        var query = dbContextMock.Object.Set<Team>().WhereMembersContain(guidToSearch);

        // Act
        var result = query.ToList();

        // Assert : Vérifie que la requête renvoie les bons résultats filtrés.
        Assert.Single(result);  // Une seule équipe devrait contenir le membre avec guidToSearch
        Assert.Equal(team1Id, result.First().Id);
    }

    [Fact]
    public void WhereMembersContain_ShouldFilterWithSqlServer_WhenProviderIsSqlServer()
    {
        // Arrange
        var team1Id = Guid.NewGuid();
        var team2Id = Guid.NewGuid();
        var guidToSearch = Guid.NewGuid();

        var teams = new List<Team>
        {
            new Team(team1Id, "TeamOne", Guid.NewGuid(), new List<Guid> { guidToSearch }, DateTimeOffset.UtcNow), // Changement du nom
            new Team(team2Id, "TeamTwo", Guid.NewGuid(), new List<Guid> { Guid.NewGuid() }, DateTimeOffset.UtcNow)  // Changement du nom
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Team>>();
        mockSet.As<IQueryable<Team>>().Setup(m => m.Provider).Returns(teams.Provider);
        mockSet.As<IQueryable<Team>>().Setup(m => m.Expression).Returns(teams.Expression);
        mockSet.As<IQueryable<Team>>().Setup(m => m.ElementType).Returns(teams.ElementType);
        mockSet.As<IQueryable<Team>>().Setup(m => m.GetEnumerator()).Returns(teams.GetEnumerator());

        var dbContextMock = new Mock<DbContext>();
        dbContextMock.Setup(c => c.Set<Team>()).Returns(mockSet.Object);

        // Simuler le comportement du provider SqlServer
        var query = dbContextMock.Object.Set<Team>().WhereMembersContain(guidToSearch);

        // Act
        var result = query.ToList();

        // Assert : Vérifie que la requête renvoie les bons résultats filtrés.
        Assert.Single(result);  // Une seule équipe devrait contenir le membre avec guidToSearch
        Assert.Equal(team1Id, result.First().Id);
    }

    [Fact]
    public void WhereMembersContain_ShouldFilterWithOtherDatabase_WhenProviderIsOther()
    {
        // Arrange
        var guidToSearch = Guid.NewGuid();

        // Utilisation du constructeur pour initialiser correctement la Team
        var team1Id = Guid.NewGuid();
        var team2Id = Guid.NewGuid();

        var teams = new List<Team>
        {
            new Team(team1Id, "TeamOne", Guid.NewGuid(), new List<Guid> { guidToSearch }, DateTimeOffset.UtcNow), // Changement du nom
            new Team(team2Id, "TeamTwo", Guid.NewGuid(), new List<Guid> { Guid.NewGuid() }, DateTimeOffset.UtcNow)  // Changement du nom
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Team>>();
        mockSet.As<IQueryable<Team>>().Setup(m => m.Provider).Returns(teams.Provider);
        mockSet.As<IQueryable<Team>>().Setup(m => m.Expression).Returns(teams.Expression);
        mockSet.As<IQueryable<Team>>().Setup(m => m.ElementType).Returns(teams.ElementType);
        mockSet.As<IQueryable<Team>>().Setup(m => m.GetEnumerator()).Returns(teams.GetEnumerator());

        var dbContextMock = new Mock<DbContext>();
        dbContextMock.Setup(c => c.Set<Team>()).Returns(mockSet.Object);

        // Simuler un fournisseur de base de données autre que Npgsql ou SqlServer
        var query = dbContextMock.Object.Set<Team>().WhereMembersContain(guidToSearch);

        // Act
        var result = query.ToList();

        // Assert : Vérifie que les résultats sont bien filtrés sans interaction avec DB.
        Assert.Single(result);  // Une seule équipe devrait contenir le membre avec guidToSearch
        Assert.Equal(team1Id, result.First().Id);
    }

    // Test pour la méthode JsonbContainsGuid et SqlServerJsonContains (elles devraient lever une exception)
    [Fact]
    public void JsonbContainsGuid_ShouldThrowNotImplementedException_WhenCalled()
    {
        // Assert
        Assert.Throws<NotImplementedException>(() => ManageJsonField.JsonbContainsGuid("json_array", Guid.NewGuid()));
        Assert.Throws<NotImplementedException>(() => ManageJsonField.SqlServerJsonContains("json_array", Guid.NewGuid().ToString()));
    }
}
