using System;
using Teams.CORE.Layer.Entities.GeneralValueObjects;
using Xunit;

namespace Teams.Tests.CORE;

public class TransfertMemberTest
{
    [Fact]
    public void AffectationStatus_Properties_AreSetCorrectly()
    {
        // Arrange
        bool isTransferAllowed = true;
        string contratType = "CDI";
        DateTime leaveDate = new DateTime(2024, 12, 31);

        // Act
        var affectationStatus = new AffectationStatus(isTransferAllowed, contratType, leaveDate);

        // Assert
        Assert.Equal(isTransferAllowed, affectationStatus.IsTransferAllowed);
        Assert.Equal(contratType, affectationStatus.ContratType);
        Assert.Equal(leaveDate, affectationStatus.LeaveDate);
    }

    [Fact]
    public void TransfertMember_Properties_AreSetCorrectly()
    {
        // Arrange
        Guid memberTeamId = Guid.NewGuid();
        string sourceTeam = "Alpha";
        string destinationTeam = "Beta";
        var affectationStatus = new AffectationStatus(false, "CDD", new DateTime(2025, 1, 1));

        // Act
        var transfertMember = new TransfertMember(
            memberTeamId,
            sourceTeam,
            destinationTeam,
            affectationStatus
        );

        // Assert
        Assert.Equal(memberTeamId, transfertMember.MemberTeamId);
        Assert.Equal(sourceTeam, transfertMember.SourceTeam);
        Assert.Equal(destinationTeam, transfertMember.DestinationTeam);
        Assert.Equal(affectationStatus, transfertMember.AffectationStatus);
    }

    [Fact]
    public void TransfertMember_AffectationStatus_IsTransferAllowed_False()
    {
        // Arrange
        var affectationStatus = new AffectationStatus(false, "CDD", DateTime.Today);
        var transfertMember = new TransfertMember(
            Guid.NewGuid(),
            "TeamA",
            "TeamB",
            affectationStatus
        );

        // Assert
        Assert.False(transfertMember.AffectationStatus.IsTransferAllowed);
    }

    [Fact]
    public void TransfertMember_AffectationStatus_ContratType_IsCorrect()
    {
        // Arrange
        var affectationStatus = new AffectationStatus(true, "Internship", DateTime.Today);
        var transfertMember = new TransfertMember(
            Guid.NewGuid(),
            "TeamX",
            "TeamY",
            affectationStatus
        );

        // Assert
        Assert.Equal("Internship", transfertMember.AffectationStatus.ContratType);
    }
}
