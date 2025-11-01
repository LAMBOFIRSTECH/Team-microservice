using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Teams.APP.Layer.Interfaces;
using Teams.APP.Layer.Services;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Dispatchers;
using Teams.INFRA.Layer.ExternalServices;
using Teams.INFRA.Layer.Interfaces;
using Teams.INFRA.Layer.Persistence.DAL;
using Teams.INFRA.Layer.Persistence.Repositories;
using Teams.INFRA.Layer.OtherUOW;

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
            services.AddDbContext<ApiContext>(opt => opt.UseInMemoryDatabase("TeamMemoryDb"));
        else
            services.AddDbContext<ApiContext>(opt => opt.UseInMemoryDatabase(conStrings));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        services.AddScoped<TeamExternalService>();
        services.AddScoped<ITeamStateUnitOfWork, TeamStateUnitOfWork>();
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddHostedService<RabbitListenerService>();
        services.AddHangfire(config => config.UseMemoryStorage());
        services.AddHangfireServer();
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
            options.Queues = HangfireQueues;
        });
        ManageRedisCacheMemory(services, configuration);
        return services;
    }
    private static readonly string[] HangfireQueues =
    {
        "default",
        "runner_operation_add_new_member",
        "runner_operation_delete_new_member",
        "runner_operation_add_project",
        "runner_operation_remove_project",
    };

private static IServiceCollection ManageRedisCacheMemory(
   this IServiceCollection services,
   IConfiguration configuration
)
    {
        var Config = configuration.GetSection("CacheSettings");
        var certPath = Config["Redis:ConfigurationOptions:Certificate:File-pfx"];
        var Password = Environment.GetEnvironmentVariable("REDISCLI_AUTH")
               ?? Config["Redis:ConfigurationOptions:Certificate:REDISCLI_AUTH"];
        if (string.IsNullOrEmpty(certPath))
            throw new Exception("No path  existing for redis client certificate");

        var clientCertificate = new X509Certificate2(certPath, Password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
        var options = new ConfigurationOptions
        {
            EndPoints = { Config["Redis:ConnectionString"]! },
            Ssl = true,
            SslHost = "172.29.0.2",
            Password = Password,
            AbortOnConnectFail = false,
            SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            AllowAdmin = true,
            ConnectTimeout = 10000,
            SyncTimeout = 10000,
            ReconnectRetryPolicy = new ExponentialRetry(5000),
        };

        options.CertificateValidation += (__, _, chain, sslPolicyErrors) =>
        {
            return sslPolicyErrors == SslPolicyErrors.None
                || (
                    sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors
                    && chain!.ChainElements[^1].Certificate.Subject == "CN=Infra local CA"
                );
        };
        options.CertificateSelection += delegate
        {
            return clientCertificate;
        };

        services.AddStackExchangeRedisCache(opts => opts.ConfigurationOptions = options);
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            try
            {
                return ConnectionMultiplexer.Connect(options);
            }
            catch (RedisConnectionException ex)
            {
                var logger = provider.GetRequiredService<ILogger<Program>>();
                logger.LogCritical(ex, "Error connecting to Redis:");
                throw;
            }
        });
        return services;
    }
}
