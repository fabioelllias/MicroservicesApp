// OrderService/Program.cs (com TraceId e SpanId nos logs)
using Microsoft.EntityFrameworkCore;
using OrderService.Extensions;
using Polly;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Contracts.Observability;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;
using OrderService.Services;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "orderservice-log-.txt");
var elasticUri = builder.Configuration["ElasticConfiguration:Uri"];

// Inicializa Serilog com TraceId e SpanId
var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .Enrich.With<ActivityEnricher>()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} TraceId={TraceId} SpanId={SpanId}{NewLine}{Exception}")
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, shared: true)
    .WriteTo.Seq("http://seq:80");

if (!string.IsNullOrEmpty(elasticUri))
{
    try
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
        Console.WriteLine("✅ Sink do Elasticsearch configurado com sucesso.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Falha ao configurar o sink do Elasticsearch: {ex.Message}");
    }
}
else
{
    Console.WriteLine("⚠️ Elasticsearch não configurado. Logs serão enviados apenas para console e arquivo.");
}

Log.Logger = loggerConfig.CreateLogger();

builder.WebHost.UseUrls("http://0.0.0.0:80");
builder.Host.UseSerilog();

// OpenTelemetry para rastreamento distribuído
Sdk.SetDefaultTextMapPropagator(new TraceContextPropagator());

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("OrderService"))
    .WithTracing(tracing => tracing
        .AddSource("OrderService")
        .AddSource("MassTransit")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o => 
        {
            o.Endpoint = new Uri("http://jaeger:4317"); // Jaeger OTLP endpoint
            o.Protocol = OtlpExportProtocol.Grpc;
        }));

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql("Host=orderdb;Port=5432;Username=postgres;Password=postgres;Database=orderdb"));

builder.Services.AddMassTransitWithRabbitMq();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCustomHealthChecks(builder.Configuration);
builder.Services.AddOpenTelemetryWithJaeger();

builder.Services.AddScoped<IOrderPublisher, OrderPublisher>();

var app = builder.Build();

// Retry com Polly para garantir que o banco esteja pronto
var retryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetry(5, attempt => TimeSpan.FromSeconds(5), (exception, time, retryCount, context) =>
    {
        Log.Warning("[Polly] Tentativa {RetryCount}: aguardando {Seconds}s - erro: {Message}", retryCount, time.TotalSeconds, exception.Message);
    });

retryPolicy.Execute(() =>
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate();
});

app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.MapControllers();
app.UseCustomMetrics();
app.UseCustomHealthChecks();

try
{
    Log.Information("🚀 OrderService iniciado");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação terminou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
