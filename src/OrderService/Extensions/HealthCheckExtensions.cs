using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using RabbitMQ.Client;
using HealthChecks.AzureServiceBus;

namespace OrderService.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var postgresConnection = configuration.GetValue<string>("PostgresSettings:ConnectionString")
                                 ?? Environment.GetEnvironmentVariable("POSTGRESSETTINGS__CONNECTIONSTRING");

        var mongoConnection = configuration.GetValue<string>("MongoSettings:ConnectionString")
                               ?? Environment.GetEnvironmentVariable("MONGOSETTINGS__CONNECTIONSTRING");

        var rabbitHost = configuration.GetValue<string>("RabbitMq:Host")
                          ?? Environment.GetEnvironmentVariable("RABBITMQ__HOST");
        var rabbitUser = configuration.GetValue<string>("RabbitMq:Username")
                          ?? Environment.GetEnvironmentVariable("RABBITMQ__USERNAME");
        var rabbitPass = configuration.GetValue<string>("RabbitMq:Password")
                          ?? Environment.GetEnvironmentVariable("RABBITMQ__PASSWORD");

        var serviceBusConnection = configuration.GetValue<string>("ServiceBus:ConnectionString")
                                    ?? Environment.GetEnvironmentVariable("SERVICEBUS__CONNECTIONSTRING");

        var builder = services.AddHealthChecks();

        if (!string.IsNullOrWhiteSpace(postgresConnection))
        {
            builder.AddNpgSql(postgresConnection, name: "PostgreSQL");
        }

        if (!string.IsNullOrWhiteSpace(mongoConnection))
        {
            builder.AddMongoDb(sp =>
            {
                var settings = MongoClientSettings.FromConnectionString(mongoConnection);
                return new MongoClient(settings);
            }, name: "MongoDB");
        }

        if (!string.IsNullOrWhiteSpace(rabbitHost) &&
            !string.IsNullOrWhiteSpace(rabbitUser) &&
            !string.IsNullOrWhiteSpace(rabbitPass))
        {
            var uri = $"amqp://{rabbitUser}:{rabbitPass}@{rabbitHost}:5672";
            builder.AddRabbitMQ(_ =>
            {
                var factory = new RabbitMQ.Client.ConnectionFactory()
                {
                    Uri = new Uri($"amqp://{rabbitUser}:{rabbitPass}@{rabbitHost}:5672")
                };
                return factory.CreateConnectionAsync();
            }, name: "RabbitMQ");
        }

        if (!string.IsNullOrWhiteSpace(serviceBusConnection))
        {
            builder.AddAzureServiceBusQueue(
                serviceBusConnection,
                queueName: "order-service-queue",
                name: "AzureServiceBus",
                timeout: TimeSpan.FromSeconds(5));
        }

        builder.Services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(15);
            setup.AddHealthCheckEndpoint("Self", "http://localhost/health");
        }).AddInMemoryStorage();

        return services;
    }

    public static IApplicationBuilder UseCustomHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseHealthChecksUI(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-ui-api";
        });

        return app;
    }
}
