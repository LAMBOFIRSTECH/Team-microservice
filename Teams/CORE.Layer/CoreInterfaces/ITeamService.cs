using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Entities.ValueObjects;

namespace Teams.CORE.Layer.CoreInterfaces;

public interface ITeamService
{
    Guid Id { get; }
    TeamName Name { get; }
    MemberId TeamManagerId { get; }
    IReadOnlyCollection<MemberId> MembersIds { get; }
    TeamState State { get; }
    bool IsTeamExpired();
    bool HasAnyDependencies();
    bool IsMature();
    void UpdateTeam(string newName, Guid newManagerId, IEnumerable<Guid> newMemberIds);
    void ArchiveTeam();
    void RemoveExpiredProjects();
    void RemoveSuspendedProjects(string projectName);
    void AddMember(Guid memberId);
    void RemoveMemberSafely(Guid memberId);
    void ChangeTeamManager(Guid newManagerId);
}