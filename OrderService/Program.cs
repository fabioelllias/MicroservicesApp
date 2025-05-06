using MassTransit;
using Microsoft.EntityFrameworkCore;
using Contracts;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:80");

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
            Console.WriteLine($"[Polly] Tentativa {retryCount}: aguardando {time.TotalSeconds}s - erro: {exception.Message}");
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
app.Run();