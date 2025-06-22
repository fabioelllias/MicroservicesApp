using MassTransit;

namespace OrderService.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitConfig = configuration.GetSection("RabbitMq");

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbitConfig["Host"], rabbitConfig["VirtualHost"], h =>
                {
                    h.Username(rabbitConfig["Username"]);
                    h.Password(rabbitConfig["Password"]);
                });
            });
        });

        return services;
    }
}
