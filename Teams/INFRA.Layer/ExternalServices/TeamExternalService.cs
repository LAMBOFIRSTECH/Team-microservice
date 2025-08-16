using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.Helpers;
using Teams.INFRA.Layer.ExternalServicesDtos;

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
        "MemberTeamId": "12345678-90ab-cdef-1234-567890abcdef",
        "SourceTeam": "Equipe de sécurité (Security Team)",
        "DestinationTeam": "Pentester",
        "AffectationStatus": {
            "IsTransferAllowed": true,
            "LeaveDate": "2025-07-03T12:34:56Z"
      }
    }

      {
        "TeamManagerId": "b14db1e2-026e-4ac9-9739-378720de6f5b",
        "TeamName": "Pentester",
        "ProjectName": "Burp suite Analysis",
        "ProjectStartDate": "2025-08-14T10:00:00Z",
        "ProjectEndDate": "2025-08-30T10:00:00Z",
        "ProjectState": {
            "State": "Active"
        }
   }

    **/

    public async Task<TransfertMemberDto?> RetrieveNewMemberToAddInRedisAsync()
    {
        var response = await httpClient.GetAsync(configuration["ExternalsApi:Employee:Url"]);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            LogHelper.Warning("No new member to add in Redis.", log);
            return null;
        }
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var root = JObject.Parse(content);
        var record = root["record"]?.ToString();
        if (record is null)
        {
            LogHelper.Warning("No record found in response.", log);
            return null;
        }
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new UtcDateTimeConverter());
        var data = JsonConvert.DeserializeObject<TransfertMemberDto>(record, settings);
        return data;
    }

    public async Task<DeleteTeamMemberDto?> RetrieveMemberToDeleteAsync()
    {
        var response = await httpClient.GetAsync(configuration["ExternalsApi:Employee:Url"]);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            LogHelper.Warning("No member to delete.", log);
            return null;
        }
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var root = JObject.Parse(content);
        var record = root["record"]?.ToString();
        if (record is null)
        {
            LogHelper.Warning("No record found in response for member deletion.", log);
            return null;
        }
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new UtcDateTimeConverter());
        var data = JsonConvert.DeserializeObject<DeleteTeamMemberDto>(record, settings);
        return data;
    }

    public async Task<ProjectAssociationDto?> RetrieveProjectAssociationDataAsync()
    {
        var response = await httpClient.GetAsync(configuration["ExternalsApi:Project:Url"]);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            LogHelper.Warning("No project association data found.", log);
            return null;
        }
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var root = JObject.Parse(content);
        var record = root["record"]?.ToString();
        if (record is null)
        {
            LogHelper.Warning("No record found in response for project association.", log);
            return null;
        }
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new UtcDateTimeConverter());
        var data = JsonConvert.DeserializeObject<ProjectAssociationDto>(record, settings);
        return data;
    }

    public class UtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime ReadJson(
            JsonReader reader,
            Type objectType,
            DateTime existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            if (reader.Value == null)
            {
                throw new JsonSerializationException("DateTime value is null.");
            }
            var dt = (DateTime)reader.Value;
            return dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
