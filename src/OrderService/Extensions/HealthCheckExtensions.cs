using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace OrderService.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var postgresConnection = configuration.GetSection("PostgresSettings")["ConnectionString"];
        var rabbitHost = configuration.GetSection("RabbitMq")["Host"];

        services.AddHealthChecks()
            .AddNpgSql(postgresConnection, name: "PostgreSQL")
            .AddRabbitMQ(_ =>
                    {
                        var factory = new RabbitMQ.Client.ConnectionFactory() {
                            Uri = new Uri($"amqp://{configuration["RabbitMq:Username"]}:{configuration["RabbitMq:Password"]}@{rabbitHost}:5672") 
                        };
                        return factory.CreateConnectionAsync();
                    }, name: "RabbitMQ");
                    
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
