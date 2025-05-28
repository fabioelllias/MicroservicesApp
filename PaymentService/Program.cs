// PaymentService/Program.cs (ajustado com TraceId, Serilog, OpenTelemetry compatível)
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService;
using Polly;
using Serilog;
using Contracts.Observability;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System.Diagnostics;

var builder = Host.CreateApplicationBuilder(args);

// Inicializa Serilog com TraceId/SpanId
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .Enrich.With<ActivityEnricher>()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} TraceId={TraceId} SpanId={SpanId}{NewLine}{Exception}")
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// Configuração do OpenTelemetry com propagação e exportação para Jaeger (versão compatível)
Sdk.SetDefaultTextMapPropagator(new TraceContextPropagator());

builder.Services.AddOpenTelemetryTracing(b => b
    .AddSource("PaymentService") // versão compatível com AddSource ao invés de AddActivitySource
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("PaymentService"))
    .AddJaegerExporter());

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql("Host=paymentdb;Port=5432;Username=postgres;Password=postgres;Database=paymentdb"));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("payment-service-queue", e =>
        {
            e.ConfigureConsumer<OrderConsumer>(ctx);
        });
    });
});

builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<OrderConsumer>();

var app = builder.Build();

// Retry com Polly para garantir que o banco esteja pronto
var retryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetry(
        retryCount: 5,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(5),
        onRetry: (exception, time, retryCount, context) =>
        {
            Log.Warning("[Polly] Tentativa {RetryCount}: aguardando {Seconds}s - erro: {Message}",
                retryCount, time.TotalSeconds, exception.Message);
        });

retryPolicy.Execute(() =>
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
});

try
{
    Log.Information("🚀 PaymentService iniciado");
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
