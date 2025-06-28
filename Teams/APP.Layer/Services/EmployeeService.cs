using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;
using Teams.CORE.Layer.Models;
using Teams.INFRA.Layer.ExternalServices;

namespace Teams.APP.Layer.Services;

public class EmployeeService(
    ITeamRepository teamRepository,
    ILogger<EmployeeService> log,
    TeamExternalService teamExternalService
) : IEmployeeService
{
    public async Task DeleteTeamMemberAsync(Guid memberId, string teamName)
    {
        throw new NotImplementedException("DeleteTeamMemberAsync method is not implemented yet.");
    }

    public async Task AddTeamMemberAsync(Guid memberId)
    {
        throw new NotImplementedException("AddTeamMemberAsync method is not implemented yet.");
    }
    // public async Task<Team> ManageTeamMemberAsync(
    //     Guid memberId,
    //     string teamName,
    //     TeamMemberAction action,
    //     Team team
    // )
    // {
    //     if (team == null)
    //         throw new DomainException($"Team '{teamName}' not found.");
    //     switch (action)
    //     {
    //         case TeamMemberAction.Add:
    //             if (team.MembersIds != null && team.MembersIds.Contains(memberId))
    //                 throw new DomainException(
    //                     $"Member '{memberId}' already exists in team '{teamName}'."
    //                 );
    //             team.AddMember(memberId);
    //             break;

    //         case TeamMemberAction.Remove:
    //             if (team.MembersIds == null || !team.MembersIds.Contains(memberId))
    //                 throw new DomainException(
    //                     $"Member '{memberId}' does not exist in team '{teamName}'."
    //                 );
    //             team.DeleteTeamMemberSafely(memberId);
    //             break;

    //         default:
    //             throw new ArgumentOutOfRangeException(nameof(action), action, null);
    //     }
    //     return team;
    // }

    // public async Task AddTeamMemberAsync(Guid memberId)
    // {
    //     var newMember = await teamExternalService.RetrieveNewMemberToAddAsync(); // dois encore vérifier newMember != null ?? Newtonsoft déjà dans Dto
    //     if (newMember == null)
    //         throw new DomainException(
    //             "Business Rule Violation",
    //             "Missing Member Data",
    //             "No new member data could be retrieved.",
    //             "Received null from RetrieveNewMemberToAddAsync."
    //         );
    //     if (!newMember.MemberTeamIdDto.Equals(memberId) || newMember.MemberTeamIdDto == Guid.Empty)
    //         throw new DomainException(
    //             "Business Rule Violation",
    //             "Member ID Mismatch",
    //             $"The provided member ID {memberId} does not match the new member's ID {newMember.MemberTeamIdDto}.",
    //             $"Expected: {newMember.MemberTeamIdDto}, Provided: {memberId}"
    //         );

    //     var team = await teamRepository.GetTeamByNameAsync(newMember.DestinationTeamDto);
    //     if (team == null)
    //         throw new DomainException(
    //             "Internal Server Error",
    //             "Team Not Found",
    //             $"No team found with Name {newMember.DestinationTeamDto}.",
    //             $"Requested Team Name: {newMember.DestinationTeamDto}"
    //         );

    //     team.CanMemberJoinNewTeam(newMember);
    //     await ManageTeamMemberAsync(
    //         newMember.MemberTeamIdDto,
    //         newMember.DestinationTeamDto,
    //         TeamMemberAction.Add,
    //         team
    //     );
    //     await teamRepository.AddTeamMemberAsync();
    // }

    // public async Task DeleteTeamMemberAsync(Guid memberId, string teamName)
    // {
    //     var teamMember = await teamRepository.GetTeamByNameAndMemberIdAsync(memberId, teamName)!;
    //     if (teamMember == null)
    //         throw new DomainException(
    //             $"A team with the name '{teamName}' not found.",
    //             "Team Name not found",
    //             "No team found with the provided name.",
    //             $"Requested Team Name: {teamName}"
    //         );
    //     try
    //     {
    //         await ManageTeamMemberAsync(memberId, teamName, TeamMemberAction.Remove, teamMember);
    //         await teamRepository.DeleteTeamMemberAsync();
    //     }
    //     catch (DomainException ex)
    //     {
    //         throw HandlerException.BadRequest(ex.Message, "Domain validation failed");
    //     }
    //}
}
