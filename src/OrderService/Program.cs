using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using OrderService.Extensions;
using OrderService.Services;
using OrderService.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// 🔗 Pipeline de configuração (JSON + Env Vars)
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 🔥 Configuração de Logs
builder.AddSerilogConfiguration();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug); // <- força MassTransit a emitir os detalhes
});


// 🗄️ Configuração de banco de dado
builder.Services.AddDbContext(builder.Configuration);

// 📦 MongoDB
builder.Services.AddMongo(builder.Configuration);

// 🐰 RabbitMQ + MassTransit
builder.Services.AddCustomMessageBroker(builder.Configuration);

// 📊 OpenTelemetry + Jaeger
builder.Services.AddOpenTelemetryConfiguration(builder.Configuration);

// ❤️ Health Checks
builder.Services.AddCustomHealthChecks(builder.Configuration);

// ♻️ Resiliência com Polly
builder.Services.AddCustomPolly(builder.Configuration);

// 📄 Swagger
builder.Services.AddSwaggerDocumentation();

// ✅ Demais serviços
builder.Services.AddControllers();
builder.Services.AddHostedService<OutboxWorker>();
//builder.Services.Configure<EventDeliverySettings>(builder.Configuration.GetSection("EventDelivery"));
builder.Services.AddEventDelivery(builder.Configuration);

builder.Services.AddScoped<IOrderPublisher, OrderPublisher>();
builder.Services.AddScoped<IExternalServiceClient, ExternalServiceClient>();
builder.Services.AddScoped<IOrderServiceClient, OrderServiceClient>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<OrderValidator>();

var app = builder.Build();

// 🔄 Migrations com Retry (Polly)
app.UseDatabaseMigration<OrderDbContext>();

// 🚀 Pipeline HTTP
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

// 🔐 Tratamento global de exceções
app.UseMiddleware<ExceptionMiddleware>();

app.UseCustomMetrics();

app.MapControllers();

app.UseCustomHealthChecks();

app.MapGet("/", () => Results.Ok("🚀 OrderService está rodando!"));

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
