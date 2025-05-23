using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService;
using Polly;

var builder = Host.CreateApplicationBuilder(args);

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
builder.Services.AddScoped<OrderConsumer>(); // REGISTRA o consumidor
var app = builder.Build();

// Retry com Polly para garantir que o banco esteja pronto
var retryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetry(
        retryCount: 5,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(5),
        onRetry: (exception, time, retryCount, context) =>
        {
            Console.WriteLine($"[Polly] Tentativa {retryCount}: aguardando {time.TotalSeconds}s - erro: {exception.Message}");
        });

retryPolicy.Execute(() =>
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
});

app.Run();