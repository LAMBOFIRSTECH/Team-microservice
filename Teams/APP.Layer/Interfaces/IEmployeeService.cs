using Teams.CORE.Layer.Models;

namespace Teams.APP.Layer.Interfaces;

public interface IEmployeeService
{
    Task<Message> AddTeamMemberAsync(Guid memberId);
}
