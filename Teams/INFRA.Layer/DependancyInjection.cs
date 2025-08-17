using System.Net.Security;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using Teams.APP.Layer.Interfaces;
using Teams.APP.Layer.Services;
using Teams.CORE.Layer.Interfaces;
using Teams.INFRA.Layer.Dispatchers;
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
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        services.AddScoped<TeamExternalService>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
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
        "runner_operation_project",
    };

    private static IServiceCollection ManageRedisCacheMemory(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var Config = configuration.GetSection("CacheSettings");
        var clientCertificate = new X509Certificate2(
            Config["Redis:ConfigurationOptions:Certificate:File-pfx"]!,
            Config["Redis:ConfigurationOptions:Certificate:REDISCLI_AUTH"],
            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet
        );

        var options = new ConfigurationOptions
        {
            EndPoints = { Config["Redis:ConnectionString"]! },
            Ssl = true,
            SslHost = "redis.infra.docker",
            Password = Config["Redis:ConfigurationOptions:Certificate:REDISCLI_AUTH"],
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
