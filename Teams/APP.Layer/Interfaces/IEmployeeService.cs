using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Models;

namespace Teams.APP.Layer.Interfaces;

public interface IEmployeeService
{
    Task AddTeamMemberAsync(Guid memberId);
    Task DeleteTeamMemberAsync(Guid memberId, string teamName);
}
