using Teams.CORE.Layer.CoreInterfaces;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Exceptions;
namespace Teams.CORE.Layer.CoreServices;

public class TeamCreationService(ITeamRepository __teamRepository) : ITeamCreationService
{
    public async Task<Team> CreateUniqueTeamAsync(string name, Guid managerId, IEnumerable<Guid> members, CancellationToken cancellationToken)
    {
        var memberIds = members as Guid[] ?? members.ToArray();
        var teams = await __teamRepository.GetTeamsByMemberIdAsync(memberIds.FirstOrDefault(), cancellationToken);
        // var managerProfile = await _employeeGateway.GetByIdAsync(managerId, cancellationToken);

        if (await __teamRepository.ExistsByNameAsync(name, cancellationToken))
            throw new BusinessRuleException("A team with the same name already exists.");

        // if (managerProfile.IsIntern)
        //     throw new BusinessRuleException("An intern cannot be a team manager.");

        var managerTeamCount = await __teamRepository.GetTeamsByManagerIdAsync(managerId, cancellationToken);
        if (managerTeamCount.Count >= 3)
            throw new BusinessRuleException("A manager cannot lead more than 3 teams.");

        if (teams.Any(t => t.MembersIds.Count == memberIds.Count() && !t.MembersIds.Select(m => m.Value).Except(memberIds).Any() && t.TeamManagerId.Value == managerId))
            throw new ConflictException("A team with exactly the same members and manager already exists.", nameof(name));

         if (GetCommonMembersStats(memberIds, teams) >= 50)
            throw new BusinessRuleException("Cannot create a team with more than 50% common members with existing team.");
        return Team.Create(name, managerId, members);
    }


    /// <summary>
    /// Calculates the maximum percentage of common members between a new team and a collection of existing teams.
    /// </summary>
    /// <param name="newTeamMembers">The list of members (as <see cref="Guid"/>) for the new team being created.</param>
    /// <param name="existingTeams">The collection of existing teams to compare against.</param>
    /// <returns>
    /// A <see cref="double"/> representing the highest percentage of overlap in members
    /// between the new team and any existing team. Returns 0 if no existing teams are provided.
    /// </returns>
    /// <exception cref="BusinessRuleException">
    /// Thrown when <paramref name="newTeamMembers"/> is null or contains fewer than two members.
    /// </exception>
    private static double GetCommonMembersStats(IEnumerable<Guid> newTeamMembers, IEnumerable<Team> existingTeams)
    {
        if (newTeamMembers == null || newTeamMembers.Count() == 0)
            throw new BusinessRuleException("The new team must have at least three members.");

        if (existingTeams == null || existingTeams.Count() == 0) return 0;
        double maxPercent = 0;
        foreach (var team in existingTeams)
        {
            var common = team.MembersIds.Select(m => m.Value).Intersect(newTeamMembers).Count();
            var universe = team.MembersIds.Select(m => m.Value).Union(newTeamMembers).Count();
            double percent = (double)common / universe * 100;
            if (percent > maxPercent) maxPercent = percent;
        }
        return maxPercent;
    }
}
// public class TeamCreationService(
//     ITeamRepository _teamRepository, 
//     IEmployeeGateway _employeeGateway) // Pour la règle 17 (contrats)
// {
//     public async Task<Team> CreateUniqueTeamAsync(string name, Guid managerId, IEnumerable<Guid> memberIds, CancellationToken ct)
//     {
//         // 1. Unicité du nom (Règle 11)
//         if (await _teamRepository.ExistsByNameAsync(name, ct))
//             throw new BusinessRuleException("A team with the same name already exists.");

//         // 2. Quota Manager (Règle 16)
//         var managerTeamCount = await _teamRepository.CountByManagerIdAsync(managerId, ct);
//         if (managerTeamCount >= 3)
//             throw new BusinessRuleException("A manager cannot lead more than 3 teams.");

//         // 3. Validation des profils (Règle 17)
//         // On vérifie via un service externe si le manager n'est pas stagiaire
//         var managerProfile = await _employeeGateway.GetByIdAsync(managerId, ct);
//         if (managerProfile.IsIntern)
//             throw new BusinessRuleException("An intern cannot be a team manager.");

//         // 4. Logique de similarité (Optionnel selon la complexité voulue)
//         // CheckSimilarityRules(memberIds);

//         // 5. Création de l'agrégat
//         return Team.Create(name, managerId, memberIds);
//     }
// }