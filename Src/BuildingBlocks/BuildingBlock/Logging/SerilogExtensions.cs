using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace BuildingBlock.Logging;

public static class SerilogExtensions
{
    // Standard structured logging for every service: compact JSON to console plus an optional
    // Seq sink, enriched with ServiceName and the current trace/span ids for log-trace correlation.
    public static IHostBuilder UseStandardSerilog(this IHostBuilder host, string serviceName) =>
        host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", serviceName)
                .WriteTo.Console(new RenderedCompactJsonFormatter());

            // Seq is optional: without configuration the service still logs to console (host dev).
            var seqUrl = context.Configuration["Seq:ServerUrl"];
            if (!string.IsNullOrWhiteSpace(seqUrl))
                loggerConfig.WriteTo.Seq(seqUrl);
        });
}
