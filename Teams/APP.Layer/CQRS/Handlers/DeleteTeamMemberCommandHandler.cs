using MediatR;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class DeleteTeamMemberCommandHandler(ITeamRepository teamRepository)
    : IRequestHandler<DeleteTeamMemberCommand>
{
    public async Task Handle(DeleteTeamMemberCommand command, CancellationToken cancellationToken)
    {
        var teams = await teamRepository.GetTeamsByMemberIdAsync(command.MemberId)!;
        if (teams == null)
            throw HandlerException.NotFound(
                $"A team with the MemberId '{command.MemberId}' not found.",
                "Team ID not found"
            );
        var teamMember = teams.FirstOrDefault(t => t.Name == command.TeamName);
        if (teamMember == null)
            throw HandlerException.NotFound(
                $"A team with the name '{command.TeamName}' not found.",
                "Team Name not found"
            );
        try
        {
            teamMember.DeleteTeamMemberSafely(command.MemberId);
        }
        catch (DomainException ex)
        {
            throw HandlerException.BadRequest(ex.Message, "Domain validation failed");
        }
        await teamRepository.UpdateTeamAsync(teamMember); // Il faut persister la suppression
        // await teamRepository.DeleteTeamMemberAsync(teamMemberId);
    }
}
