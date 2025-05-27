using Microsoft.EntityFrameworkCore;
using OrderService.Extensions;
using Polly;
using Prometheus;
using Serilog;
using Serilog.Sinks.Elasticsearch;


var builder = WebApplication.CreateBuilder(args);

var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "orderservice-log-.txt");
// 🔧 Lê configuração do appsettings ou variáveis de ambiente
var elasticUri = builder.Configuration["ElasticConfiguration:Uri"];

// 🔧 Inicializa Serilog
var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
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
            conn.ServerCertificateValidationCallback((o, certificate, chain, errors) => true) // <- Bypass
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

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql("Host=orderdb;Port=5432;Username=postgres;Password=postgres;Database=orderdb"));

builder.Services.AddMassTransitWithRabbitMq(); // extensão
builder.Services.AddOpenTelemetryWithJaeger(); // extensão


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCustomHealthChecks(builder.Configuration); // extensão

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
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate();
});

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.UseRouting();

app.UseCustomMetrics(); // extensão
app.UseCustomHealthChecks(); // extensão

try{
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
