using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using JwtAuthLibrary;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Teams.API.Layer.Mappings;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Validators;
using Teams.APP.Layer.Services;
using Teams.CORE.Layer.Interfaces;
using Teams.INFRA.Layer.Persistence;
using Teams.INFRA.Layer.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    });

builder.Services.AddEndpointsApiExplorer();
builder
    .Configuration.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "API.Layer"))
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: false,
        reloadOnChange: false
    )
    .AddEnvironmentVariables();

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc(
        builder.Configuration["ApiVersion"],
        new OpenApiInfo
        {
            Title = "Team Management service | Api",
            Description = "An ASP.NET Core Web API for managing Teams",
            Version = builder.Configuration["ApiVersion"],
            Contact = new OpenApiContact { Name = "Artur Lambo", Email = "lamboartur94@gmail.com" },
        }
    );

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var conStrings = builder.Configuration.GetSection("ConnectionStrings")["DefaultConnection"];
if (string.IsNullOrEmpty(conStrings))
{
    throw new ArgumentException(
        "Connection string 'DefaultConnection' is not set in appsettings.json"
    );
}
var kestrelSection = builder.Configuration.GetSection("Kestrel:EndPoints:Https");
var certificateFile = kestrelSection["Certificate:File"];
var certificatePassword = kestrelSection["Certificate:CertPassword"];
var caCertFile = kestrelSection["Certificate:CAFile"];

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
        // 📜 Charge le certificat serveur pour HTTPS
        if (
            string.IsNullOrEmpty(certificateFile)
            || string.IsNullOrEmpty(certificatePassword)
            || !File.Exists(certificateFile)
        )
            throw new InvalidOperationException("Le certificat serveur est requis pour HTTPS.");

        var serverCertificate = new X509Certificate2(certificateFile, certificatePassword);
        httpsOptions.ServerCertificate = serverCertificate;
        // 🔐 Active le mode de certificat client
        httpsOptions.ClientCertificateMode = Enum.Parse<ClientCertificateMode>(
            kestrelSection["ClientCertificateMode"] ?? "RequireCertificate",
            ignoreCase: true
        );

        // vérifier que le certificat client est signé et validé par le bon CA
        httpsOptions.ClientCertificateValidation = (cert, chain, errors) =>
        {
            if (string.IsNullOrEmpty(caCertFile) || !File.Exists(caCertFile))
            {
                throw new ArgumentException(
                    "CA certificate file path 'Kestrel:EndPoints:Https:Certificate:CAFile' is not set in configuration."
                );
            }
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

builder.Services.AddDbContext<TeamDbContext>(opt => opt.UseInMemoryDatabase(conStrings));
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();
builder.Services.AddRouting();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();
builder.Services.AddHealthChecks();
builder.Logging.AddConsole();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<EmployeService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateTeamCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateTeamCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddAutoMapper(typeof(TeamProfile).Assembly);
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

// builder.Services.AddAutoMapper(cfg =>
// {
//     cfg.AddProfile<TeamProfile>();
// }, AppDomain.CurrentDomain.GetAssemblies()); //protéger l'assemblage de mappage pour lever les exceptions de mappage
builder.Services.AddAuthorization();
builder
    .Services.AddAuthentication("JwtAuthorization")
    .AddScheme<JwtBearerOptions, JwtBearerAuthenticationMiddleware>(
        "JwtAuthorization",
        options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                ValidAudience = builder.Configuration["JwtSettings:Audience"],
                ClockSkew = TimeSpan.Zero,
            };
        }
    );

builder.Services.AddAuthorization(options =>
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

// Configuration de OpenTelemetry pour la traçabilité
builder
    .Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("api-teams"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(
                    $"http://{builder.Configuration["Jaeger:IpAddress"]}:{builder.Configuration["Jaeger:Port"]}"
                );
                otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            });
    });

var app = builder.Build();
app.Map(
    "/team-management",
    teamApp =>
    {
        teamApp.UseRouting();
        teamApp.UseAuthentication();
        teamApp.UseAuthorization();
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
    }
);
await app.RunAsync();
