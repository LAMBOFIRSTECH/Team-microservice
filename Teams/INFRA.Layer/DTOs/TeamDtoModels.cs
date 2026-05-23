namespace Teams.INFRA.Layer.DTOs;
public class TeamDtoModels
{
    public record TeamDto(Guid Id, string Name, Guid TeamManagerId, IEnumerable<Guid> MembersIds);
    public record TeamDetailsDto(Guid Id, string Name, Guid TeamManagerId, string TeamCreationDate, string TeamExpirationDate, bool HasAnyProject, List<string>? ProjectNames, string State);
    public record DeleteTeamMemberDto(Guid MemberId, string? TeamName);
}