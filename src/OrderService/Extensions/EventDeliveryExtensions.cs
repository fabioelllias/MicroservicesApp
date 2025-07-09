using OrderService.Configurations;

namespace OrderService.Extensions;

public static class EventDeliveryExtensions
{
    public static IServiceCollection AddEventDelivery(this IServiceCollection services, IConfiguration configuration)
    {
        var eventDeliverySettings = configuration.GetSection("EventDeliverySettings").Get<EventDeliverySettings>(); 

        // Fallback: variável de ambiente EVENTDELIVERY__STRATEGY
        if (string.IsNullOrWhiteSpace(eventDeliverySettings?.Strategy))
        {
            eventDeliverySettings ??= new EventDeliverySettings(); // garante instância não nula
            eventDeliverySettings.Strategy = Environment.GetEnvironmentVariable("EVENTDELIVERY__STRATEGY");

            Console.WriteLine("⚠️ Fallback: lendo EventDelivery do Environment => " +
                                  (string.IsNullOrEmpty(eventDeliverySettings.Strategy) ? "(vazio)" : "✅ encontrado"));
        }        

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
