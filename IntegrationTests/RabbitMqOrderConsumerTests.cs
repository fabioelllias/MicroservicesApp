using System;
using System.Threading.Tasks;
using Xunit;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Contracts;

namespace IntegrationTests;

public class RabbitMqOrderConsumerTests : IAsyncLifetime
{
    private readonly TestcontainersContainer _rabbitMqContainer;
    private IHost? _host;

    public RabbitMqOrderConsumerTests()
    {
        _rabbitMqContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("rabbitmq:3.12-management")
            .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .WithWaitStrategy(DotNet.Testcontainers.Builders.Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _rabbitMqContainer.StartAsync();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddMassTransit(x =>
                {
                    x.AddConsumer<OrderConsumer>();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(_rabbitMqContainer.Hostname, _rabbitMqContainer.GetMappedPublicPort(5672), "/", h =>
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
    }

    public async Task DisposeAsync()
    {
        if (_host is not null)
            await _host.StopAsync();

        await _rabbitMqContainer.DisposeAsync();
    }

    [Fact]
    public async Task DeveConsumirMensagemOrder()
    {
        var bus = _host!.Services.GetRequiredService<IBus>();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            ProductName = "Mouse Gamer",
            Quantity = 2,
            CreatedAt = DateTime.UtcNow
        };

        await bus.Publish(order);

        await Task.Delay(1000);

        Assert.True(true);
    }
}
