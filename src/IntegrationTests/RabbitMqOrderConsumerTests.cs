using System;
using System.Threading.Tasks;
using Xunit;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Contracts;
using PaymentService;

public class RabbitMqOrderConsumerTests : IAsyncLifetime
{
    private TestcontainersContainer _rabbitMqContainer;
    private TestcontainersContainer _postgresContainer;
    private IHost? _host;

    public RabbitMqOrderConsumerTests()
    {
        _rabbitMqContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("rabbitmq:3.12-management")
            .WithPortBinding(5672, true)
            .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
            .WithWaitStrategy(DotNet.Testcontainers.Builders.Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();

        _postgresContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("postgres:16")
            .WithPortBinding(5432, true)
            .WithEnvironment("POSTGRES_DB", "payment")
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_PASSWORD", "postgres")
            .WithWaitStrategy(DotNet.Testcontainers.Builders.Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _rabbitMqContainer.StartAsync();
        await _postgresContainer.StartAsync();

        var rabbitPort = _rabbitMqContainer.GetMappedPublicPort(5672);
        var postgresPort = _postgresContainer.GetMappedPublicPort(5432);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<PaymentDbContext>(options =>
                    options.UseNpgsql($"Host=localhost;Port={postgresPort};Database=payment;Username=postgres;Password=postgres"));

                services.AddMassTransit(x =>
                {
                    x.AddConsumer<OrderConsumer>();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host("localhost", rabbitPort, "/", h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.ReceiveEndpoint("order-queue", e =>
                        {
                            e.ConfigureConsumer<OrderConsumer>(context);
                        });
                    });
                });

                services.AddLogging();
            })
            .Build();

        await _host.StartAsync();

        // Cria o schema no banco
        using var scope = _host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_host is not null)
            await _host.StopAsync();

        await _rabbitMqContainer.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task DevePersistirPagamentoNoBanco()
    {
        var bus = _host!.Services.GetRequiredService<IBus>();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            ProductName = "Teclado Mecânico",
            Quantity = 1,
            CreatedAt = DateTime.UtcNow
        };

        await bus.Publish(order);

        await Task.Delay(1500); // espera o consumo

        using var scope = _host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        var pagamento = await db.Payments.FirstOrDefaultAsync(p => p.OrderId == order.Id);
        Assert.NotNull(pagamento);
    }
}
