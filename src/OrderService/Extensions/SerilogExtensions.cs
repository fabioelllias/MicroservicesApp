using Contracts.Observability;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Microsoft.ApplicationInsights.Extensibility;

namespace OrderService.Extensions;

public static class SerilogExtensions
{
    public static void AddSerilogConfiguration(this WebApplicationBuilder builder)
    {

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.With<ActivityEnricher>()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} TraceId={TraceId} SpanId={SpanId}{NewLine}{Exception}");

        if (builder.Environment.IsDevelopment())
        {
            var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "orderservice-log-.txt");

            var elasticUri = builder.Configuration["ElasticConfiguration:Uri"];
            var seqUri = builder.Configuration["SeqConfiguration:Uri"];

            // Somente em DEV: logs em arquivo, Seq e Elastic
            loggerConfig.WriteTo.File(logPath, rollingInterval: RollingInterval.Day, shared: true);

            if (!string.IsNullOrWhiteSpace(seqUri))
                loggerConfig.WriteTo.Seq(seqUri);

            if (!string.IsNullOrWhiteSpace(elasticUri))
            {
                loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "orderservice-log-{0:yyyy.MM.dd}",
                    ModifyConnectionSettings = conn =>
                        conn.ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
                            .DisableAutomaticProxyDetection(true)
                            .EnableDebugMode()
                            .ThrowExceptions()
                });
            }
        }
        else
        {
            // Em produção: Application Insights (se configurado)
            var appinsightsConnection = Environment.GetEnvironmentVariable("APPINSIGHTS__CONNECTIONSTRING");

            if (!string.IsNullOrWhiteSpace(appinsightsConnection))
            {
                var telemetryConfiguration = new TelemetryConfiguration
                {
                    ConnectionString = appinsightsConnection
                };

                loggerConfig.WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces);
            }
        }

        Log.Logger = loggerConfig.CreateLogger();
        builder.Host.UseSerilog();
    }
}
