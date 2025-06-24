using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.Interfaces;
using Teams.CORE.Layer.Models;

namespace Teams.APP.Layer.Services;

public class EmployeeService(
    HttpClient httpClient,
    ITeamRepository teamRepository,
    ILogger<EmployeeService> log
) : IEmployeeService
{
    /**
    https://jsonbin.io/quick-store/
    {
        "MemberId": "23456789-0abc-def1-2345-67890abcdef1",
        "SourceTeam": "Equipe de sécurité (Security Team)",
        "DestinationTeam": "Equipe de recherche et d'innovation (RnD Team)",
        "AffectationStatus": {
            "IsTransferAllowed": true,
            "LastLeaveDate": "2025-06-10T12:34:56Z"
      }
    }
    **/
    public async Task<TransfertMemberDto?> RetrieveNewMemberToAddAsync()
    {
        var response = await httpClient.GetAsync(
            "https://api.jsonbin.io/v3/qs/685868a18960c979a5af5069"
        );
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var root = JObject.Parse(content);
        var record = root["record"]?.ToString();
        if (record is null)
            return null;
        var data = JsonConvert.DeserializeObject<TransfertMemberDto>(record);
        return data;
    }

    private async Task<(bool, Message?)> CanMemberJoinNewTeam(TransfertMemberDto transfertMemberDto)
    {
        var teams = await teamRepository.GetTeamsByMemberIdAsync(transfertMemberDto!.MemberId);
        if (teams == null || teams.Count == 0)
            return (true, null); // Le membre n'existe pas dans une équipe
        if (transfertMemberDto.AffectationStatus.IsTransferAllowed.Equals(false))
            return (
                false,
                new Message
                {
                    Status = 500,
                    Detail =
                        $"The team member {transfertMemberDto.MemberId} cannot be added in a new team.",
                    Type = "Internal Server Error",
                    Title = "Not allow member",
                }
            );
        // Le membre existe dans une équipe et ne peut pas être transféré
        var daysSinceDeparture = DateTime.UtcNow - transfertMemberDto.AffectationStatus.LeaveDate;
        if (daysSinceDeparture.TotalDays < 7)
            return (false, null); // Moins de 7 jours : refus
        return (true, null);
    }

    public async Task<Message> AddTeamMemberAsync(Guid memberId)
    {
        var newMember = await RetrieveNewMemberToAddAsync();
        if (newMember.MemberId.Equals(memberId) is false)
            return new Message
            {
                Type = "Business Rule Violation",
                Title = "Member ID Mismatch",
                Detail =
                    $"The provided member ID {memberId} does not match the new member's ID {newMember.MemberId}.",
                Status = 400,
            };
        var result = await CanMemberJoinNewTeam(newMember!);
        if (!result.Item1)
            return new Message
            {
                Type = "Business Rule Violation",
                Title = "Member Cooldown Period",
                Detail =
                    $"member {newMember!.MemberId} must wait 7 days before being added to a new team.",
                Status = 400,
            };

        await teamRepository.AddTeamMemberByDetailsAsync(
            newMember!.MemberId,
            newMember.DestinationTeam
        );
        return new Message
        {
            Type = "Success",
            Title = "Member Added",
            Detail =
                $"Member {newMember.MemberId} has been successfully added to team {newMember.DestinationTeam}.",
            Status = 201,
        };
    }
}
