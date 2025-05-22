using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;


var builder = WebApplication.CreateBuilder(args);

var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "orderservice-log-.txt");

// 🔧 Inicializa Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, shared: true)
    .WriteTo.Seq("http://seq:80")
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "orderservice-logs-{0:yyyy.MM.dd}",
        ModifyConnectionSettings = conn =>
            conn.ServerCertificateValidationCallback((o, certificate, chain, errors) => true) // <- Bypass
               .DisableAutomaticProxyDetection(true)
               .EnableDebugMode()
               .ThrowExceptions()
    })
    .CreateLogger();
   
Serilog.Debugging.SelfLog.Enable(Console.Error);

builder.WebHost.UseUrls("http://0.0.0.0:80");

builder.Host.UseSerilog();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql("Host=orderdb;Port=5432;Username=postgres;Password=postgres;Database=orderdb"));

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
