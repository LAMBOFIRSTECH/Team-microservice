using System.Net;
using System.Net.Http.Json;
using Teams.CORE.Layer.CoreInterfaces;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Teams.CORE.Layer.Exceptions;
using Teams.INFRA.Layer.ExternalServices.ProjectService.DTOs.Input;

namespace Teams.INFRA.Layer.ExternalServices.ProjectService;

public class ProjectProvider : IProjectProvider
{
    private readonly HttpClient _httpClient;

    public ProjectProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProjectAssociation> RetrieveProjectForTeamAsync(Guid projectId)
    {
        var response = await _httpClient.GetAsync($"api/external-projects/{projectId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new ExternalServiceException($"Project {projectId} was not found.");

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<ProjectAssociationDto>();

        if (dto is null)
            throw new ExternalServiceException("External project payload is null.");

        // 1. Validation MINIMALE du contrat externe (structure uniquement)
        ValidateDto(dto);

        // 2. Mapping → domaine (le domaine fait le reste des règles)
        var domainDetails = dto.Details
            .Select(d => new Detail(
                d.ProjectName,
                d.ProjectStartDate,
                d.ProjectEndDate,
                d.VoState.State
            ))
            .ToList();

        return new ProjectAssociation(
            dto.ProjectId,
            dto.TeamManagerId,
            dto.TeamName,
            domainDetails
        );
    }

    private static void ValidateDto(ProjectAssociationDto dto)
    {
        if (dto.ProjectId == Guid.Empty)
            throw new ExternalServiceException("Invalid ProjectId.");

        if (dto.TeamManagerId == Guid.Empty)
            throw new ExternalServiceException("Invalid TeamManagerId.");

        if (string.IsNullOrWhiteSpace(dto.TeamName))
            throw new ExternalServiceException("Invalid TeamName.");

        if (dto.Details is null || dto.Details.Count == 0)
            throw new ExternalServiceException("At least one detail is required.");

        foreach (var d in dto.Details)
        {
            if (d is null)
                throw new ExternalServiceException("Null detail found.");

            if (string.IsNullOrWhiteSpace(d.ProjectName))
                throw new ExternalServiceException("Invalid ProjectName.");

            if (d.VoState is null)
                throw new ExternalServiceException("Missing project state.");

            if (!Enum.IsDefined(typeof(VoState), d.VoState.State))
                throw new ExternalServiceException("Invalid project state from external system.");
        }
    }
}