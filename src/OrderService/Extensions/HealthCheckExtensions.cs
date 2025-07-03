using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace OrderService.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var postgresConnection = configuration.GetSection("PostgresSettings")["ConnectionString"];
        if (string.IsNullOrWhiteSpace(postgresConnection))
        {
            postgresConnection = Environment.GetEnvironmentVariable("POSTGRESSETTINGS__CONNECTIONSTRING");
            Console.WriteLine("⚠️ Fallback: lendo ConnectionString do PostgreSQL do Environment => " + postgresConnection);
        }

        var rabbitHost = configuration["RabbitMq:Host"];
        if (string.IsNullOrWhiteSpace(rabbitHost))
        {
            rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ__HOST");
            Console.WriteLine("⚠️ Fallback: lendo RabbitMQ Host do Environment => " + rabbitHost);
        }

        var rabbitUser = configuration["RabbitMq:Username"];
        if (string.IsNullOrWhiteSpace(rabbitUser))
        {
            rabbitUser = Environment.GetEnvironmentVariable("RABBITMQ__USERNAME");
        }

        var rabbitPass = configuration["RabbitMq:Password"];
        if (string.IsNullOrWhiteSpace(rabbitPass))
        {
            rabbitPass = Environment.GetEnvironmentVariable("RABBITMQ__PASSWORD");
        }

        if (string.IsNullOrWhiteSpace(postgresConnection))
        {
            Console.WriteLine("❌ String de conexão do PostgreSQL não encontrada. HealthCheck não será configurado.");
            return services;
        }

        services.AddHealthChecks()
            .AddNpgSql(postgresConnection, name: "PostgreSQL");

        // Se houver dados de conexão com RabbitMQ
        if (!string.IsNullOrWhiteSpace(rabbitHost) &&
            !string.IsNullOrWhiteSpace(rabbitUser) &&
            !string.IsNullOrWhiteSpace(rabbitPass))
        {
            services.AddHealthChecks().AddRabbitMQ(_ =>
            {
                var factory = new RabbitMQ.Client.ConnectionFactory()
                {
                    Uri = new Uri($"amqp://{rabbitUser}:{rabbitPass}@{rabbitHost}:5672")
                };
                return factory.CreateConnectionAsync();
            }, name: "RabbitMQ");
        }
        else
        {
            Console.WriteLine("⚠️ RabbitMQ não configurado para HealthCheck.");
        }

        services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(15);
            setup.AddHealthCheckEndpoint("Self", "/health");
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
