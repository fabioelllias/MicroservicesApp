using MassTransit;

namespace OrderService.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddCustomMessageBroker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Registra todos os consumers do assembly atual (você pode ajustar isso)
            //x.AddConsumers(AppDomain.CurrentDomain.GetAssemblies());

            bool IsDevelopment = configuration.GetSection("RabbitMq")["Host"] == "rabbitmq";

            if (IsDevelopment)
            {
                var rabbitConfig = configuration.GetSection("RabbitMq");

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(rabbitConfig["Host"], rabbitConfig["VirtualHost"], h =>
                    {
                        h.Username(rabbitConfig["Username"]);
                        h.Password(rabbitConfig["Password"]);
                    });

                    cfg.ConfigureEndpoints(ctx);
                });
            }
            else
            {
                var serviceBusConnection = Environment.GetEnvironmentVariable("SERVICEBUS__CONNECTIONSTRING");

                if (string.IsNullOrWhiteSpace(serviceBusConnection))
                    throw new InvalidOperationException("❌ A connection string do Azure Service Bus não foi encontrada.");

                x.UsingAzureServiceBus((ctx, cfg) =>
                {
                    cfg.Host(serviceBusConnection);
                    cfg.ConfigureEndpoints(ctx);
                });
            }
        });

        return services;
    }
}
