using OrderService.Configurations;

namespace OrderService.Extensions;

public static class EventDeliveryExtensions
{
    public static IServiceCollection AddEventDelivery(this IServiceCollection services, IConfiguration configuration)
    {
        var eventDeliverySettings = configuration
            .GetSection("EventDelivery")
            .Get<EventDeliverySettings>() ?? new EventDeliverySettings();

        // Fallback: variável de ambiente EVENTDELIVERY__STRATEGY
        eventDeliverySettings.Strategy ??= Environment.GetEnvironmentVariable("EVENTDELIVERY__STRATEGY");

        if (string.IsNullOrWhiteSpace(eventDeliverySettings.Strategy))
        {
            Console.WriteLine("⚠️ EventDelivery.Strategy não configurado. Usando 'Direct' como padrão.");
            eventDeliverySettings.Strategy = "Direct";
        }
        else
        {
            Console.WriteLine($"✅ EventDelivery.Strategy => {eventDeliverySettings.Strategy}");
        }

        services.AddSingleton(eventDeliverySettings);
        return services;

    }
}
