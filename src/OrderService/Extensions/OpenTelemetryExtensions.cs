using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

namespace OrderService.Extensions;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetryConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var jaegerEndpoint = configuration.GetSection("OpenTelemetry")["JaegerEndpoint"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("OrderService"))
            .WithTracing(tracing => tracing
                .AddSource("OrderService")
                .AddSource("MassTransit")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(jaegerEndpoint);
                    o.Protocol = OtlpExportProtocol.Grpc;
                }));

        return services;
    }
}
