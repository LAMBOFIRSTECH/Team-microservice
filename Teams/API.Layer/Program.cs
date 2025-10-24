using System.Security.Cryptography.X509Certificates;
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Serilog;
using Teams.API.Layer;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer;
using Teams.INFRA.Layer;


var builder = WebApplication.CreateBuilder(args);
builder
    .Configuration.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "INFRA.Layer"))
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: false,
        reloadOnChange: false
    )
    .AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddApiDI(builder.Configuration);
builder.Services.AddApplicationDI(builder.Configuration);
builder.Services.AddInfrastructureDI(builder.Configuration);

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
        {
            throw new InvalidOperationException("Le certificat serveur est requis pour HTTPS.");
        }

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
            var chain2 = new X509Chain
            {
                ChainPolicy = new X509ChainPolicy
                {
                    RevocationMode = X509RevocationMode.NoCheck,
                    RevocationFlag = X509RevocationFlag.ExcludeRoot,
                    TrustMode = X509ChainTrustMode.CustomRootTrust,
                },
            };
            chain2.ChainPolicy.CustomTrustStore.Add(caCert);
            return chain2.Build(cert);
        };
    });
});

builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();
builder.Services.AddRouting();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();
builder.Services.AddHealthChecks();

var app = builder.Build();
try
{
    Log.Information("🟢 Application starting up");
    app.Map(
        "/team-management",
        teamApp =>
        {
            teamApp.UseRouting();
            teamApp.UseAuthentication();
            teamApp.UseAuthorization();
            teamApp.UseSwagger();
            teamApp.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/team-management/swagger/{app.Configuration["ApiVersion"]}/swagger.json", "Team Management API");
                c.RoutePrefix = "swagger";
            });
            teamApp.UseMiddleware<ExceptionMiddleware>();
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

            // Hangfire Dashboard sécurisé
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
            teamApp.UseMiddleware<RequestLoggingMiddleware>();
        }
    );

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}
