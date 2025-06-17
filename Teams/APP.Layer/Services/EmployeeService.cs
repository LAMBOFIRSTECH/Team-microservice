using Newtonsoft.Json;
using Teams.API.Layer.DTOs;

namespace Teams.APP.Layer.Services;

public class EmployeeService
{
    private readonly HttpClient _httpClient;

    public EmployeeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DateTime?> GetLastTeamLeaveDateAsync(Guid memberId)
    {
        var response = await _httpClient.GetAsync(
            "https://recherche-entreprises.api.gouv.fr/search?q=84292630500058"
        );
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null; // Pas d’historique trouvé

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        string json =
            @"{
                ""memberId"": ""a22b89a7-01ab-40d8-8904-b5f1ceadbd90"",
                ""lastLeaveDate"": ""2025-06-07T12:34:56Z""
            }";

        var data = JsonConvert.DeserializeObject<MemberLeaveDateDto>(json);

        return data?.LastLeaveDate;
    }
}
