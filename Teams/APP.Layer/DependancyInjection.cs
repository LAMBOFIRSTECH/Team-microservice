using CustomVaultPackage;
using FluentValidation;
using Teams.APP.Layer.Mappings;
using Teams.APP.Layer.CQRS.Validators;
using Teams.APP.Layer.Interfaces;
using Teams.APP.Layer.Services;
using Teams.APP.Layer.Services.Scheldulers;
using Teams.CORE.Layer.CoreServices;
using Teams.APP.Layer.DomainHandlers;
using Teams.APP.Layer.EventHandlers;
using Teams.CORE.Layer.CoreInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Teams.APP.Layer.FeatureTeam.CreateTeam;
using Teams.APP.Layer.FeatureTeam.UpdateTeam;
using Teams.APP.Layer.FeatureTeam.UpdateTeamByManager;
namespace Teams.APP.Layer;

public static class DependancyInjection
{
    public static IServiceCollection AddApplicationDI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining<CreateTeamValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateTeamValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateTeamByManagerValidator>();
        // services.AddValidatorsFromAssemblyContaining<TransfertMemberRecordValidator>();
        services.AddAutoMapper(typeof(TeamProfile).Assembly);
        services.AddAutoMapper(typeof(ProjectProfile).Assembly);
        services.AddAutoMapper(typeof(TransfertMemberProfile).Assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(TeamDomainHandler).Assembly);
        });
        // // Enregistrement automatique de tous les IDomainEventHandler<T> | quand on ajoute un nouveau handler, pas besoin de l'enregistrer manuellement
        var handlerInterface = typeof(IDomainEventHandler<>);
        var assemblies = new[] { typeof(TeamCreatedEventHandler).Assembly }; // ou plusieurs assemblies si besoin

        foreach (var type in assemblies.SelectMany(a => a.GetTypes()).Where(t => !t.IsInterface && !t.IsAbstract))
        {
            var interfaces = type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface);
            foreach (var iface in interfaces)
                services.AddScoped(iface, type);
            
        }
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ITeamCreationService, TeamCreationService>();
        services.AddScoped<ITeamProjectLifeCycle, TeamProjectLifeCycle>();
        services.AddSingleton<ProjectExpiryScheduler>();
        services.AddSingleton<IProjectExpirySchedule>(sp => sp.GetRequiredService<ProjectExpiryScheduler>());
        services.AddHostedService(sp => sp.GetRequiredService<ProjectExpiryScheduler>());
        services.AddScoped<ProjectLifeCycle>();

        services.AddSingleton<TeamExpiryScheduler>();
        services.AddSingleton<ITeamExpiryScheduler>(sp => sp.GetRequiredService<TeamExpiryScheduler>());
        services.AddHostedService(sp => sp.GetRequiredService<TeamExpiryScheduler>());

        services.AddSingleton<TeamMaturityScheduler>();
        services.AddSingleton<ITeamMaturityScheduler>(sp => sp.GetRequiredService<TeamMaturityScheduler>());
        services.AddHostedService(sp => sp.GetRequiredService<TeamMaturityScheduler>());
        AddAuthorizationPolicies(services);
        AddOpenTelemetryTracing(services, configuration);

        return services;
    }
    private static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                "AdminPolicy",
                policy =>
                    policy
                        .RequireRole(nameof(Rule.Privilege.Administrateur))
                        .RequireAuthenticatedUser()
                        .AddAuthenticationSchemes("JwtAuthorization")
            );
            options.AddPolicy(
                "ManagerPolicy",
                policy =>
                    policy
                        .RequireRole(nameof(Rule.Privilege.Manager))
                        .RequireAuthenticatedUser()
                        .AddAuthenticationSchemes("JwtAuthorization")
            );
        });
        return services;
    }

    private static IServiceCollection AddOpenTelemetryTracing(this IServiceCollection services,IConfiguration configuration)
    {
        var ipAddress = configuration.GetSection("Jaeger")["IpAddress"];
        var port = configuration.GetSection("Jaeger")["Port"];
        if (string.IsNullOrWhiteSpace(ipAddress) || string.IsNullOrWhiteSpace(port))
        {
            throw new InvalidOperationException(
                "Jaeger IP address or port is not configured correctly."
            );
        }
        services
            .AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("api-teams"))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(otlpOptions =>
                    {
                        var endpoint = new Uri($"https://{ipAddress}:{port}");
                        otlpOptions.Endpoint = endpoint;
                        otlpOptions.Protocol = OpenTelemetry
                            .Exporter
                            .OtlpExportProtocol
                            .HttpProtobuf;
                    });
            });

        return services;
    }
}
