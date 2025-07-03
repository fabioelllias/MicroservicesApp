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

        // Fallback para variável de ambiente
        if (string.IsNullOrWhiteSpace(jaegerEndpoint))
        {
            jaegerEndpoint = Environment.GetEnvironmentVariable("OPENTELEMETRY__JAEGERENDPOINT");
            Console.WriteLine("⚠️ Fallback: lendo JaegerEndpoint do Environment => " + jaegerEndpoint);
        }

        if (string.IsNullOrWhiteSpace(jaegerEndpoint))
        {
            Console.WriteLine("❌ JaegerEndpoint não encontrado. Telemetria não será configurada.");
            return services; // Retorna sem configurar o OpenTelemetry
        }

        // Configuração segura
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