using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService;
using NotificationService.Data;
using Polly;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    config.SetMinimumLevel(LogLevel.Debug);
});

// Banco de dados
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<NotificationDbContext>(options =>
        options.UseNpgsql("Host=notificationdb;Port=5432;Username=postgres;Password=postgres;Database=notificationdb"));
}
else
{
    var postgresConnection = Environment.GetEnvironmentVariable("POSTGRESSETTINGS__CONNECTIONSTRING");

    if (string.IsNullOrWhiteSpace(postgresConnection))
        throw new InvalidOperationException("❌ A connection string do PostgreSQL não foi encontrada.");

    builder.Services.AddDbContext<NotificationDbContext>(options =>
        options.UseNpgsql(postgresConnection));
}

builder.Services.AddScoped<OrderConsumer>();

// Mensageria
if (builder.Environment.IsDevelopment())
{
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

            cfg.ReceiveEndpoint("notification-service-queue", e =>
            {
                e.ConfigureConsumer<OrderConsumer>(ctx);
            });
        });
    });
}
else
{
    var serviceBusConnection = Environment.GetEnvironmentVariable("SERVICEBUS__CONNECTIONSTRING");

    if (string.IsNullOrWhiteSpace(serviceBusConnection))
        throw new InvalidOperationException("❌ A connection string do Azure Service Bus não foi encontrada.");

    builder.Services.AddMassTransit(x =>
     {
         x.AddConsumer<OrderConsumer>();

         x.UsingAzureServiceBus((ctx, cfg) =>
         {
             cfg.Host(serviceBusConnection);

             cfg.Message<Order>(x => x.SetEntityName("notification-service-queue"));

             cfg.ReceiveEndpoint("notification-service-queue", e =>
             {
                 e.ConfigureConsumeTopology = false;
                 e.ConfigureConsumer<OrderConsumer>(ctx);

                 e.UseMessageRetry(r => r.Intervals(
                     TimeSpan.FromSeconds(10),
                     TimeSpan.FromSeconds(30),
                     TimeSpan.FromMinutes(1)
                 ));
             });
         });
     });

}

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Migração automática apenas em dev
if (builder.Environment.IsDevelopment())
{
    var retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetry(5, i => TimeSpan.FromSeconds(5),
            (ex, t, i, ctx) => Console.WriteLine($"[Polly] Retry {i}: {ex.Message}"));

    retryPolicy.Execute(() =>
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        db.Database.Migrate();
    });
}

app.Run();
