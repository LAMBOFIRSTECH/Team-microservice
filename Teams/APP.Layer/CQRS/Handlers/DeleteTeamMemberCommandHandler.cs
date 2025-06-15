using MediatR;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class DeleteTeamMemberCommandHandler(ITeamRepository teamRepository)
    : IRequestHandler<DeleteTeamMemberCommand>
{
    public async Task Handle(DeleteTeamMemberCommand command, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetTeamsByMemberIdAsync(command.MemberId)!;
        if (team == null)
            throw new HandlerException(
                404,
                $"A team with the MemberId '{command.MemberId}' not found.",
                "Not Found",
                "Team ID not found"
            );
        var teamMember = team.FirstOrDefault(t => t.Name == command.TeamName);
        if (teamMember!.MemberId.FirstOrDefault().Equals(teamMember.TeamManagerId))
            throw new HandlerException(
                400,
                $"The team member with the MemberId '{command.MemberId}' cannot be deleted because they are the team manager.",
                "Bad Request",
                "Cannot delete team manager"
            );
        await teamRepository.DeleteTeamMemberAsync(teamMember!.MemberId.FirstOrDefault());
    }
}
