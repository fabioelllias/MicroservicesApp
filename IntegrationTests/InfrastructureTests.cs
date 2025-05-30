
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Configurations;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests;

public class InfrastructureTests : IAsyncLifetime
{
    private readonly PostgreSqlTestcontainer _postgresContainer;
    private readonly RabbitMqTestcontainer _rabbitMqContainer;

    public InfrastructureTests()
    {
        _postgresContainer = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "testdb",
                Username = "testuser",
                Password = "testpass"
            })
            .Build();

        _rabbitMqContainer = new TestcontainersBuilder<RabbitMqTestcontainer>()
            .WithMessageBroker(new RabbitMqTestcontainerConfiguration
            {
                Username = "guest",
                Password = "guest"
            })
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
    }

[Fact]
public void ContainersDevemEstarDisponiveis()
{
    Assert.NotNull(_postgresContainer.Hostname);
    Assert.True(_postgresContainer.GetMappedPublicPort(5432) > 0);

    Assert.NotNull(_rabbitMqContainer.Hostname);
    Assert.True(_rabbitMqContainer.GetMappedPublicPort(5672) > 0);
}

}
