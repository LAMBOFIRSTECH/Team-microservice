using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.Services;

public class EmployeeService(
    HttpClient httpClient,
    ITeamRepository teamRepository,
    ILogger<EmployeeService> log
)
{
    public async Task<MemberLeaveDateDto?> RetrieveNewMemberToAddAsync()
    {
        var response = await httpClient.GetAsync(
            "https://api.jsonbin.io/v3/qs/68573c448960c979a5aeaf46"
        );
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null; // Pas d’historique trouvé

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var root = JObject.Parse(content);
        var record = root["record"]?.ToString();

        if (record is null)
            return null;

        var data = JsonConvert.DeserializeObject<MemberLeaveDateDto>(record);
        return data;
    }

    private async Task<bool> CanMemberJoinNewTeam(MemberLeaveDateDto memberLeaveDateDto)
    {
        var teams = await teamRepository.GetTeamsByMemberIdAsync(memberLeaveDateDto!.MemberId);
        if (teams == null || teams.Count == 0)
            return true; // Le membre n'existe pas dans une équipe
        log.LogCritical("voici la date : {LastLeaveDate}", memberLeaveDateDto.LastLeaveDate);
        var daysSinceDeparture = DateTime.UtcNow - memberLeaveDateDto.LastLeaveDate;
        if (daysSinceDeparture.TotalDays < 7)
            return false; // Moins de 7 jours : refus
        return true; // Le membre n'était dans aucune équipe
    }

    public async Task AddTeamMemberAsync() //pb ici 
    {
        var newMember = await RetrieveNewMemberToAddAsync();
        log.LogWarning("voici la date : {LastLeaveDate}", newMember.LastLeaveDate);
        if (!await CanMemberJoinNewTeam(newMember!))
            throw new HandlerException(
                400,
                $"member {newMember!.MemberId} must wait 7 days before being added to a new team.",
                "Business Rule Violation",
                "Member Cooldown Period"
            );

        await teamRepository.AddTeamMemberByDetailsAsync(newMember!.MemberId, newMember.TeamName);
    }
}
