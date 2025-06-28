using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.ExternalServicesDtos;

namespace Teams.INFRA.Layer.ExternalServices;

public class TeamExternalService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<TeamExternalService> log
)
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
        var response = await httpClient.GetAsync(configuration["ExternalServices:Employee:Url"]);
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

    public async Task<DeleteTeamMemberDto?> RetrieveMemberToDeleteAsync()
    {
        var response = await httpClient.GetAsync(configuration["ExternalServices:Employee:Url"]);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var root = JObject.Parse(content);
        var record = root["record"]?.ToString();
        if (record is null)
            return null;
        var data = JsonConvert.DeserializeObject<DeleteTeamMemberDto>(record);
        return data;
    }

    public async Task<ProjectAssociationDto?> RetrieveProjectAssociationDataAsync()
    {
        var response = await httpClient.GetAsync(configuration["ExternalServices:Project:Url"]);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var root = JObject.Parse(content);
        var record = root["record"]?.ToString();
        if (record is null)
            return null;
        var data = JsonConvert.DeserializeObject<ProjectAssociationDto>(record);
        return data;
    }
}
