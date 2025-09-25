using System.Reflection;
using CustomVaultPackage;
using FluentValidation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Teams.API.Layer.Mappings;
using Teams.APP.Layer.CQRS.Validators;
using Teams.APP.Layer.Interfaces;
using Teams.APP.Layer.Services;

namespace Teams.APP.Layer;

public static class DependancyInjection
{
    public static IServiceCollection AddApplicationDI(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddValidatorsFromAssemblyContaining<CreateTeamCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateTeamCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateTeamManagerValidator>();
        services.AddValidatorsFromAssemblyContaining<TransfertMemberRecordValidator>();
        services.AddValidatorsFromAssemblyContaining<ProjectRecordValidator>();
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddAutoMapper(typeof(TeamProfile).Assembly);
        services.AddAutoMapper(typeof(ProjectProfile).Assembly);
        services.AddAutoMapper(typeof(TransfertMemberProfile).Assembly);

        // services.AddMediatR(cfg =>
        // {
        //     cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        // });
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
        });

        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddScoped<ProjectService>();
        services.AddSingleton<ProjectExpiryScheduler>();
        services.AddSingleton<IProjectExpirySchedule>(sp =>
            sp.GetRequiredService<ProjectExpiryScheduler>()
        );
        services.AddHostedService(sp => sp.GetRequiredService<ProjectExpiryScheduler>());
        services.AddSingleton<TeamExpiryScheduler>();
        services.AddSingleton<ITeamExpiryScheduler>(sp =>
            sp.GetRequiredService<TeamExpiryScheduler>()
        );
        services.AddHostedService(sp => sp.GetRequiredService<TeamExpiryScheduler>());
        services.AddSingleton<TeamMaturityScheduler>();
        services.AddSingleton<ITeamMaturityScheduler>(sp =>
            sp.GetRequiredService<TeamMaturityScheduler>()
        );
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

    private static IServiceCollection AddOpenTelemetryTracing(
        this IServiceCollection services,
        IConfiguration configuration
    )
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
