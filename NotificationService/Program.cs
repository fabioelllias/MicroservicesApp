using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService;
using NotificationService.Data;
using Polly;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql("Host=notificationdb;Port=5432;Username=postgres;Password=postgres;Database=notificationdb"));

builder.Services.AddScoped<OrderConsumer>();
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

        cfg.ReceiveEndpoint("order-queue", e =>
        {
            e.ConfigureConsumer<OrderConsumer>(ctx);
        });
    });
});

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

var retryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetry(5, i => TimeSpan.FromSeconds(5), (ex, t, i, ctx) =>
    {
        Console.WriteLine($"[Polly] Retry {i}: {ex.Message}");
    });

retryPolicy.Execute(() =>
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    db.Database.Migrate();
});

app.Run();
