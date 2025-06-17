using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Interfaces;
using Teams.INFRA.Layer.Persistence;
using Teams.INFRA.Layer.Persistence.Repositories;

namespace Teams.INFRA.Layer;

public static class DependancyInjection
{
    public static IServiceCollection AddInfrastructureDI(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var conStrings = configuration.GetSection("ConnectionStrings")["DefaultConnection"];
        if (string.IsNullOrEmpty(conStrings) || conStrings == "TeamMemoryDb")
        {
            services.AddDbContext<TeamDbContext>(opt => opt.UseInMemoryDatabase("TeamMemoryDb"));
        }
        services.AddDbContext<TeamDbContext>(opt => opt.UseInMemoryDatabase(conStrings));
        services.AddScoped<ITeamRepository, TeamRepository>();
        return services;
    }
}
