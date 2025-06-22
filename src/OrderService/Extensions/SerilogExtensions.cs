using Contracts.Observability;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace OrderService.Extensions;

public static class SerilogExtensions
{
    public static void AddSerilogConfiguration(this WebApplicationBuilder builder)
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "orderservice-log-.txt");
        var elasticUri = builder.Configuration["ElasticConfiguration:Uri"];
        var seqUri = builder.Configuration["SeqConfiguration:Uri"];

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.With<ActivityEnricher>()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} TraceId={TraceId} SpanId={SpanId}{NewLine}{Exception}")
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, shared: true);

        if (!string.IsNullOrEmpty(seqUri))
            loggerConfig.WriteTo.Seq(seqUri);

        if (!string.IsNullOrEmpty(elasticUri))
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

        Log.Logger = loggerConfig.CreateLogger();
        builder.Host.UseSerilog();
    }
}
