using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using Teams.APP.Layer.CQRS.Handlers.Events;
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
        else
        {
            services.AddDbContext<TeamDbContext>(opt => opt.UseInMemoryDatabase(conStrings));
        }
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<TeamEvent>();
        services.AddHangfire(config => config.UseMemoryStorage());
        services.AddHangfireServer();
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 2;
            options.Queues = new[] { "default", "getnewmemberfromexternalapi" };
        });
        return services;
    }
}
