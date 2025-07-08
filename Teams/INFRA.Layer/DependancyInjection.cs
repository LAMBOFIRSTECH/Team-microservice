using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Teams.APP.Layer.CQRS.Events;
using Teams.APP.Layer.Interfaces;
using Teams.APP.Layer.Services;
using Teams.CORE.Layer.Interfaces;
using Teams.INFRA.Layer.ExternalServices;
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
            services.AddDbContext<TeamDbContext>(opt => opt.UseInMemoryDatabase("TeamMemoryDb"));
        else
            services.AddDbContext<TeamDbContext>(opt => opt.UseInMemoryDatabase(conStrings));

        services.AddScoped<ITeamRepository, TeamRepository>();
        // services.AddScoped<EmployeeCreatedEventHandler>();
        services.AddScoped<TeamExternalService>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddHostedService<RabbitListenerService>();
        services.AddHangfire(config => config.UseMemoryStorage());
        services.AddHangfireServer();
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 3;
            options.Queues = HangfireQueues;
        });
        return services;
    }

    private static readonly string[] HangfireQueues =
    {
        "default",
        "runner_operation_add_new_member",
        "runner_operation_delete_new_member",
        "runner_operation_project",
    };
}
