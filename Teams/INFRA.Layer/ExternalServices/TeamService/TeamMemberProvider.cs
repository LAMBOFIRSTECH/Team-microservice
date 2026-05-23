using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Teams.INFRA.Layer.DTOs.Input;
using Teams.INFRA.Layer.Helperss;

namespace Teams.INFRA.Layer.ExternalServices;
public class TeamMemberProvider( HttpClient _httpClient, IConfiguration _configuration,ILogger<TeamMemberProvider> _log)
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
