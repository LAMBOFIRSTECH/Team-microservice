using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Teams.APP.Layer.Configurations;

public static class OpenTelemetryConfiguration
{
    public static IServiceCollection AddOpenTelemetryTracing(
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
                        // Construire l'URL avec vérification des valeurs
                        var endpoint = new Uri($"http://{ipAddress}:{port}");
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
