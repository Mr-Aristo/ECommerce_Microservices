using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlock.Observability;

public static class OpenTelemetryExtensions
{
    // Standard tracing + metrics for every service. The OTLP exporter reads its target from the
    // OTEL_EXPORTER_OTLP_ENDPOINT env var; when unset (host dev) it stays a no-op.
    public static IServiceCollection AddStandardOpenTelemetry(this IServiceCollection services, string serviceName)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("MassTransit")   // checkout integration-event flow (Basket -> Order)
                .AddSource("Npgsql")        // Marten/PostgreSQL (Catalog, Basket)
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter());

        return services;
    }
}
