using Microsoft.EntityFrameworkCore;
using Teams.CORE.Entities.TeamAG;
namespace Teams.INFRA.Persistence.DAL.EFMapping;

public static class ManageJsonField
{
    public static IQueryable<Team> WhereMembersContain(this IQueryable<Team> query, Guid guid)
    {
        var providerName = query.Provider.GetType().Name;
        if (providerName.Contains("Npgsql"))
            return query.Where(e => JsonbContainsGuid(EF.Property<string>(e, nameof(Team.TeamMembers)), guid));
        
        else if (providerName.Contains("SqlServer"))
            return query.Where(e => SqlServerJsonContains(EF.Property<string>(e, nameof(Team.TeamMembers)),guid.ToString()));
        else
            return query.AsEnumerable().Where(e => e.TeamMembers.Select(m => m.MemberId!.Value).Contains(guid)).AsQueryable();  
    }

    [DbFunction("jsonb_contains_guid", IsBuiltIn = false)]
    public static bool JsonbContainsGuid(string jsonArray, Guid guid)
        => throw new NotSupportedException("We use this method to manage SQL on (PostgreSQL).");


    [DbFunction("JSON_CONTAINS_GUID", IsBuiltIn = false)]
    public static bool SqlServerJsonContains(string jsonArray, string guid)
        => throw new NotSupportedException("We use this method to manage SQL on (SQL Server).");

}
