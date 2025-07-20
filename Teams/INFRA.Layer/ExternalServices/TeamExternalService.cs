using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Teams.API.Layer.DTOs;
using Teams.INFRA.Layer.ExternalServicesDtos;

namespace Teams.INFRA.Layer.ExternalServices;

public class TeamExternalService(HttpClient httpClient, IConfiguration configuration)
{
    /**
    https://jsonbin.io/quick-store/
    {
        "MemberId": "12345678-90ab-cdef-1234-567890abcdef",
        "SourceTeam": "Equipe de sécurité (Security Team)",
        "DestinationTeam": "Pentester",
        "AffectationStatus": {
            "IsTransferAllowed": true,
            "LeaveDate": "2025-07-03T12:34:56Z"
      }
    }

    {
        "TeamManagerId": "c5b8e9b6-4a19-4a53-a41e-8b6f4d1a5d74",
        "TeamName": "Pentester",
        "ProjectStartDate": "2025-07-20T10:00:00Z",
        "ProjectState": {
            "State": "Active"
        }
    }

    **/

    public async Task<TransfertMemberDto?> RetrieveNewMemberToAddInRedisAsync()
    {
        var response = await httpClient.GetAsync(configuration["ExternalsApi:Employee:Url"]);
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
        var response = await httpClient.GetAsync(configuration["ExternalsApi:Employee:Url"]);
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
        var response = await httpClient.GetAsync(configuration["ExternalsApi:Project:Url"]);
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
