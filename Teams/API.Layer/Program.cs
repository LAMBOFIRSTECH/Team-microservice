using System.Security.Cryptography.X509Certificates;
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Teams.API.Layer;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer;
using Teams.APP.Layer.Configurations;
using Teams.APP.Layer.CQRS.Handlers;
using Teams.CORE.Layer;
using Teams.INFRA.Layer;

var builder = WebApplication.CreateBuilder(args);
builder
    .Configuration.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "API.Layer"))
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: false,
        reloadOnChange: false
    )
    .AddEnvironmentVariables();
foreach (var kvp in builder.Configuration.AsEnumerable())
{
    if (kvp.Value?.StartsWith("${") == true && kvp.Value.EndsWith("}"))
    {
        var envVarName = kvp.Value.Trim(new char[] { '$', '{', '}' });
        var envValue = Environment.GetEnvironmentVariable(envVarName);
        if (!string.IsNullOrEmpty(envValue))
        {
            builder.Configuration[kvp.Key] = envValue;
        }
    }
}

// Ajouter les services via les DI respectives
builder.Services.AddApiDI(builder.Configuration);
builder.Services.AddApplicationDI();
builder.Services.AddInfrastructureDI(builder.Configuration);
builder.Services.AddCoreDI();

// Configuration HTTPS et certificats
var kestrelSection = builder.Configuration.GetSection("Kestrel:EndPoints:Https");
var certificateFile = kestrelSection["Certificate:File"];
var certificatePassword = kestrelSection["Certificate:CertPassword"];
var caCertFile = kestrelSection["Certificate:CAFile"];

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
        if (
            string.IsNullOrEmpty(certificateFile)
            || string.IsNullOrEmpty(certificatePassword)
            || !File.Exists(certificateFile)
        )
            throw new InvalidOperationException("Le certificat serveur est requis pour HTTPS.");

        var serverCertificate = new X509Certificate2(certificateFile, certificatePassword);
        httpsOptions.ServerCertificate = serverCertificate;

        httpsOptions.ClientCertificateMode = Enum.Parse<ClientCertificateMode>(
            kestrelSection["ClientCertificateMode"] ?? "RequireCertificate",
            ignoreCase: true
        );

        httpsOptions.ClientCertificateValidation = (cert, chain, errors) =>
        {
            if (string.IsNullOrEmpty(caCertFile) || !File.Exists(caCertFile))
                throw new ArgumentException(
                    "CA certificate file path 'Kestrel:EndPoints:Https:Certificate:CAFile' is not set in configuration."
                );

            var caCert = new X509Certificate2(caCertFile);
            var chain2 = new X509Chain();
            chain2.ChainPolicy = new X509ChainPolicy
            {
                RevocationMode = X509RevocationMode.NoCheck,
                RevocationFlag = X509RevocationFlag.ExcludeRoot,
                TrustMode = X509ChainTrustMode.CustomRootTrust,
            };
            chain2.ChainPolicy.CustomTrustStore.Add(caCert);
            return chain2 != null && chain2.Build(cert);
        };
    });
});

// Ajouter d'autres services comme HttpClient, Routing, Health Checks, etc.
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();
builder.Services.AddRouting();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();
builder.Services.AddHealthChecks();
builder.Services.AddLogging();

// Ajouter l'authentification et l'autorisation
builder.Services.AddAuthorizationPolicies(); // Déplacé dans APP.Layer

// Ajouter OpenTelemetry
builder.Services.AddOpenTelemetryTracing(builder.Configuration);

var app = builder.Build();
app.Map(
    "/team-management",
    teamApp =>
    {
        teamApp.UseRouting();
        teamApp.UseAuthentication();
        teamApp.UseAuthorization();
        teamApp.UseMiddleware<ExceptionHandlerMiddleware>();

        teamApp.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health");
            endpoints.MapGet(
                "/version",
                async context =>
                {
                    var version =
                        app.Configuration.GetValue<string>("ApiVersion") ?? "Version not set";
                    await context.Response.WriteAsync(version);
                }
            );
        });

        var HangFireConfig = app.Configuration.GetSection("HangfireCredentials");
        teamApp.UseHangfireDashboard(
            "/hangfire",
            new DashboardOptions()
            {
                DashboardTitle = "Hangfire Dashboard for Lamboft Inc ",
                Authorization = new[]
                {
                    new BasicAuthAuthorizationFilter(
                        new BasicAuthAuthorizationFilterOptions
                        {
                            Users = new[]
                            {
                                new BasicAuthAuthorizationUser
                                {
                                    Login = HangFireConfig["UserName"],
                                    PasswordClear = HangFireConfig["HANGFIRE_PASSWORD"],
                                },
                            },
                        }
                    ),
                },
            }
        );
    }
);
RecurringJob.AddOrUpdate<ManageTeamEventHandler>(
    "job-check-and-add-member",
    handler => handler.HandleAsync(),
    Cron.Hourly,
    new RecurringJobOptions { QueueName = "getnewmemberfromexternalapi" }
);

await app.RunAsync();
