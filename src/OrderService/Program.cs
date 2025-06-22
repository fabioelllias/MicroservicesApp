using Microsoft.EntityFrameworkCore;
using Serilog;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;
using FluentValidation;
using FluentValidation.AspNetCore;
using Contracts.Observability;
using OrderService.Extensions;
using OrderService.Configurations;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔗 Pipeline de configuração (JSON + Env Vars)
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 🔥 Configuração de Logs
builder.AddSerilogConfiguration();

// 🗄️ Configuração de banco de dado
builder.Services.AddDbContext(builder.Configuration);


// 📦 MongoDB
builder.Services.AddMongo(builder.Configuration);

// 🐰 RabbitMQ + MassTransit
builder.Services.AddRabbitMq(builder.Configuration);

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
builder.Services.Configure<EventDeliverySettings>(builder.Configuration.GetSection("EventDelivery"));

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

app.UseCustomMetrics();

app.MapControllers();

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
