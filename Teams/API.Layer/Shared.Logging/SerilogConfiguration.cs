using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Teams.API.Layer.Shared.Logging;

public static class SerilogConfiguration
{
    public static void ConfigureLogging(IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithCorrelationId()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .WriteTo.Console(
                theme: AnsiConsoleTheme.Sixteen,
                restrictedToMinimumLevel: LogEventLevel.Debug
            )
            .CreateLogger();
    }
}
