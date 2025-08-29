using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.Helpers;
using Teams.INFRA.Layer.ExternalServicesDtos;

namespace Teams.INFRA.Layer.ExternalServices;

public class TeamExternalService(
    HttpClient _httpClient,
    IConfiguration _configuration,
    ILogger<TeamExternalService> _log
)
{
    private async Task<string> GetContent(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            LogHelper.Warning("No data found.", _log);
            return null!;
        }
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var root = JObject.Parse(content);
        var record = root["record"]?.ToString();
        if (record is null)
        {
            LogHelper.Warning("No record found in response.", _log);
            return null!;
        }
        return record;
    }

    public async Task<TransfertMemberDto?> RetrieveNewMemberToAddInRedisAsync()
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            _configuration["ExternalsApi:Employee:Url"]
        );
        request.Headers.Add(
            "X-Master-Key",
            _configuration["ExternalsApi:Employee:Headers:X-Access-Key"]
        );
        var record = await GetContent(request);
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new UtcDateTimeConverter());
        var data = JsonConvert.DeserializeObject<TransfertMemberDto>(record, settings);
        return data;
    }

    public async Task<DeleteTeamMemberDto?> RetrieveMemberToDeleteAsync()
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            _configuration["ExternalsApi:Employee:Url"]
        );
        request.Headers.Add(
            "X-Master-Key",
            _configuration["ExternalsApi:Employee:Headers:X-Access-Key"]
        );
        var record = await GetContent(request);
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new UtcDateTimeConverter());
        var data = JsonConvert.DeserializeObject<DeleteTeamMemberDto>(record, settings);
        return data;
    }

    public async Task<ProjectAssociationDto?> RetrieveProjectAssociationDataAsync()
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            _configuration["ExternalsApi:Project:Url"]
        );
        request.Headers.Add(
            "X-Master-Key",
            _configuration["ExternalsApi:Project:Headers:X-Access-Key"]
        );
        var record = await GetContent(request);
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

        public override void WriteJson(
            JsonWriter writer,
            DateTime value,
            JsonSerializer serializer
        ) => serializer.Serialize(writer, value);
    }
}
