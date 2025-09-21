using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities;

namespace Teams.INFRA.Layer.Persistence.EFQueries;

public static class HashSetJsonExtensions
{
    public static IQueryable<Team> WhereMembersContain(this IQueryable<Team> query, Guid guid)
    {
        var providerName = query.Provider.GetType().Name;

        if (providerName.Contains("Npgsql"))
        {
            return query.Where(e =>
                JsonbContainsGuid(EF.Property<string>(e, nameof(Team.MembersIds)), guid)
            );
        }
        else if (providerName.Contains("SqlServer"))
        {
            return query.Where(e =>
                SqlServerJsonContains(
                    EF.Property<string>(e, nameof(Team.MembersIds)),
                    guid.ToString()
                )
            );
        }
        else
        {
            return query.AsEnumerable().Where(e => e.MembersIds.Select(m => m.Value).Contains(guid)).AsQueryable();
        }
    }

    [DbFunction("jsonb_contains_guid", IsBuiltIn = false)]
    public static bool JsonbContainsGuid(string jsonArray, Guid guid)
    {
        throw new NotImplementedException(
            "We use this method to manage SQL on  (PostgreSQL)."
        );
    }

    [DbFunction("JSON_CONTAINS_GUID", IsBuiltIn = false)]
    public static bool SqlServerJsonContains(string jsonArray, string guid)
    {
        throw new NotImplementedException(
            "We use this method to manage SQL on (SQL Server)."
        );
    }
}
