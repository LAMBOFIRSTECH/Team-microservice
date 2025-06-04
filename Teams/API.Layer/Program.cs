using System.Reflection;
using Microsoft.OpenApi.Models;
using Teams.API.Layer.Middlewares;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Teams.CORE.Layer.Interfaces;
using Teams.INFRA.Layer.Persistence.Repositories;
using FluentValidation.AspNetCore;
using FluentValidation;
using Teams.APP.Layer.Interfaces;
using Teams.INFRA.Layer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Configuration
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "API.Layer"))
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables();


builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc(builder.Configuration["Kestrel:ApiVersion"], new OpenApiInfo
    {
        Title = "Team Management service | Api",
        Description = "An ASP.NET Core Web API for managing Teams",
        Version = builder.Configuration["Kestrel:ApiVersion"],
        Contact = new OpenApiContact
        {
            Name = "Artur Lambo",
            Email = "lamboartur94@gmail.com"
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();
builder.Services.AddRouting();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IHashicorpVaultService, HashicorpVaultService>();

builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters(); //Nécessaire pour la validation des commandes
builder.Services.AddValidatorsFromAssemblyContaining<Teams.APP.Layer.CQRS.Commands.CreateTeamCommand>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
// builder.Services.AddMediatR(cfg =>
// {
//     cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
//     cfg.RegisterServicesFromAssembly(typeof(Teams.Core.Layer.Entities.Team).Assembly);
//     cfg.RegisterServicesFromAssembly(typeof(Teams.APP.Layer.CQRS.Commands.CreateTeamCommand).Assembly);
// });


builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("api-teams"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri($"http://{builder.Configuration["Jaeger:IpAddress"]}:{builder.Configuration["Jaeger:Port"]}");
                otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;

            });
    });

var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseMiddleware<ContextPathMiddleware>("/team-management");
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Debug");
    app.UseHsts();
    app.UseSwagger();
    app.UseSwaggerUI(con =>
    {
        con.SwaggerEndpoint("/team-management/swagger/v1.0/swagger.yml", "Gestion des équipes");

        con.RoutePrefix = string.Empty;
    });
}


app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHealthChecks("/health");
    endpoints.MapGet("/version", async context =>
    {
        var version = app.Configuration.GetValue<string>("Kestrel:ApiVersion") ?? "Version not set";
        await context.Response.WriteAsync(version);
    });
});

await app.RunAsync();
